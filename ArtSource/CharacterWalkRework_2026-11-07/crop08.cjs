const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');(async()=>{let p=__dirname+'/Character08/';let im=sharp(p+'cartoon-walk-A-v1.png'),m=await im.metadata();await im.extract({left:Math.round(m.width*4/5),top:0,width:m.width-Math.round(m.width*4/5),height:Math.round(m.height/4)}).png().toFile(p+'identity-reference.png');})();

