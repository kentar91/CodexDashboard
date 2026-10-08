const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.env.CODEXDASHBOARD_SHARP || 'sharp');
const root = path.resolve(__dirname, '..', 'assets', 'branding');
async function main() {
 for(const kind of ['symbol','icon','logo']) {
  const source=fs.readFileSync(path.join(root,'codex-dashboard-'+kind+'.svg'));
  await sharp(source).resize(kind==='logo'?1440:1024).png().toFile(path.join(root,'codex-dashboard-'+kind+'.png'));
 }
 const master=fs.readFileSync(path.join(root,'codex-dashboard-icon.svg'));
 const sizes=[16,24,32,48,64,72,128,144,256,512];
 const images=[];
 for(const size of sizes) {
  const png=await sharp(master).resize(size,size).png().toBuffer();
  fs.writeFileSync(path.join(root,'codex-dashboard-icon-'+size+'.png'),png);
  if(size<=256&&size!==72&&size!==144)images.push({size,png});
 }
 const header=Buffer.alloc(6+16*images.length);header.writeUInt16LE(1,2);header.writeUInt16LE(images.length,4);
 let offset=header.length;
 images.forEach(({size,png},i)=>{const at=6+i*16;header[at]=size===256?0:size;header[at+1]=size===256?0:size;header.writeUInt16LE(1,at+4);header.writeUInt16LE(32,at+6);header.writeUInt32LE(png.length,at+8);header.writeUInt32LE(offset,at+12);offset+=png.length;});
 fs.writeFileSync(path.join(root,'CodexDashboard.ico'),Buffer.concat([header,...images.map(x=>x.png)]));
 for(const [name,color] of [['white','#FFFFFF'],['black','#000000']]) {
  const svg=fs.readFileSync(path.join(root,'codex-dashboard-symbol.svg'),'utf8').replace(/#F3E600|#55EAD4/g,color);
  fs.writeFileSync(path.join(root,'codex-dashboard-symbol-'+name+'.svg'),svg);
  await sharp(Buffer.from(svg)).resize(1024).png().toFile(path.join(root,'codex-dashboard-symbol-'+name+'.png'));
 }
 await sharp(fs.readFileSync(path.join(root,'codex-dashboard-logo.svg'))).flatten({background:'#000000'}).png().toFile(path.join(root,'codex-dashboard-logo-dark.png'));
 const tiles=await Promise.all([32,64,144,256].map(async(size,i)=>({input:await sharp(master).resize(size,size).png().toBuffer(),left:24+i*300+Math.floor((256-size)/2),top:24+Math.floor((256-size)/2)})));
 await sharp({create:{width:1220,height:304,channels:4,background:'#161616'}}).composite(tiles).png().toFile(path.join(root,'icon-sizes-preview.png'));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
