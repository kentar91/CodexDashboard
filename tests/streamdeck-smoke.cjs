// Simulate the documented Stream Deck WebSocket without touching user profiles.
const net = require('node:net');
const crypto = require('node:crypto');
const {spawn} = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const exe = path.resolve(__dirname, '../build/streamdeck/com.codexdashboard.monitor.sdPlugin/CodexDashboard.exe');
fs.mkdirSync(path.resolve(__dirname, '../build/reports'), {recursive:true});
const contexts = ['short-test', 'weekly-test', 'open-test'];
const frames = new Map();
let registered = false, refreshed = false, localized = false, child, peer, originalOpen;
function send(socket, value) {
 const b = Buffer.from(JSON.stringify(value));
 let head;
 if(b.length < 126) head = Buffer.from([0x81, b.length]);
 else {head = Buffer.alloc(4); head[0] = 0x81; head[1] = 126; head.writeUInt16BE(b.length, 2);}
 socket.write(Buffer.concat([head,b]));
}
const server = net.createServer(socket => {
 peer = socket; let handshake = false, pending = Buffer.alloc(0);
 socket.on('error', () => {});
 socket.on('data', chunk => {
  pending = Buffer.concat([pending,chunk]);
  if(!handshake) {
   const end = pending.indexOf('\r\n\r\n'); if(end === -1) return;
   const key = /Sec-WebSocket-Key: (.+)\r\n/i.exec(pending.subarray(0,end+4).toString())[1];
   const accept = crypto.createHash('sha1').update(key+'258EAFA5-E914-47DA-95CA-C5AB0DC85B11').digest('base64');
   socket.write('HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: '+accept+'\r\n\r\n');
   handshake = true; pending = pending.subarray(end+4);
  }
  while(pending.length >= 2) {
   const opcode = pending[0] & 15; let length = pending[1]&127, start=2;
   if(length === 126) {if(pending.length<4)return; length=pending.readUInt16BE(2);start=4;}
   if(length === 127) throw new Error('Unexpected large WebSocket frame');
   assert.ok(pending[1]&128, 'Client frames must be masked');
   if(pending.length < start+4+length)return;
   const mask=pending.subarray(start,start+4), body=Buffer.from(pending.subarray(start+4,start+4+length));
   for(let i=0;i<body.length;i++)body[i]^=mask[i%4];
   pending=pending.subarray(start+4+length);
   if(opcode !== 1)continue;
   const m=JSON.parse(body.toString());
   if(m.event==='registerPlugin') {
    assert.equal(m.uuid,'com.codexdashboard.monitor'); registered=true;
    contexts.forEach((context,i)=>send(socket,{event:'willAppear',context,action:'com.codexdashboard.monitor.'+['short','weekly','open'][i],payload:{}}));
   } else if(m.event==='setImage') {
    assert.ok(contexts.includes(m.context)); assert.equal(m.payload.target,0);
    assert.ok(m.payload.image.startsWith('data:image/png;base64,'));
    const png=Buffer.from(m.payload.image.split(',')[1],'base64');
    assert.equal(png.subarray(0,8).toString('hex'),'89504e470d0a1a0a');
    assert.equal(png.readUInt32BE(16),144);assert.equal(png.readUInt32BE(20),144);frames.set(m.context,png);
    if(frames.size===3&&!refreshed){originalOpen=frames.get('open-test');send(socket,{event:'keyDown',context:contexts[0],action:'com.codexdashboard.monitor.short',payload:{}});send(socket,{event:'didReceiveSettings',context:'open-test',action:'com.codexdashboard.monitor.open',payload:{settings:{language:'en'}}});refreshed=true;}
    if(refreshed&&m.context==='open-test'&&!originalOpen.equals(png)){localized=true;finish();}
   }
  }
 });
});
let finished=false;
function finish(error) {
 if(finished)return; finished=true; clearTimeout(timeout);
 if(peer)peer.destroy();server.close();
 setTimeout(()=>{if(child&&!child.killed)child.kill();},1500).unref();
 if(error){console.error(error);process.exitCode=1;return;}
 assert.ok(registered&&refreshed&&localized);
 for(const [context,png] of frames)fs.writeFileSync(path.resolve(__dirname,'../build/reports/streamdeck-'+context+'.png'),png);
 fs.writeFileSync(path.resolve(__dirname,'../build/reports/streamdeck-smoke.txt'),'PASS: registration, masked WebSocket frames, three contexts, 144x144 PNG images, refresh command, per-key language update, disconnect.\n');
 console.log('PASS: Stream Deck registration, all three action images, and refresh command.');
}
const timeout=setTimeout(()=>finish(new Error('Stream Deck simulation timed out')),45000);
server.listen(0,'127.0.0.1',()=>{
 child=spawn(exe,['-port',String(server.address().port),'-pluginUUID','com.codexdashboard.monitor','-registerEvent','registerPlugin','-info','{}'],{windowsHide:true});
 child.on('error',finish);
 child.on('exit',code=>{if(!finished)finish(new Error('Plugin exited early: '+code));});
});
