from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
HERE=Path(__file__).resolve().parent
names=[('north_sea','北海'),('mediterranean_sea','地中海'),('africa','非洲'),('indian_ocean','印度洋'),('southeast_asia','东南亚 · 原版基准'),('east_asia','东亚'),('new_world','新大陆')]
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',23)
sheet=Image.new('RGB',(1440,7*300),(18,28,38));draw=ImageDraw.Draw(sheet)
for row,(region,label) in enumerate(names):
    for col,sky in enumerate(['Sunny','Sunset','Night']):
        image=Image.open(HERE/'Review'/f'{region}-{sky}.png').resize((480,270),Image.Resampling.LANCZOS)
        sheet.paste(image,(col*480,row*300+30))
        draw.text((col*480+8,row*300+1),label+' / '+{'Sunny':'晴天','Sunset':'夕阳','Night':'夜晚'}[sky],font=font,fill=(235,224,196))
sheet.save(HERE/'Review/seven-regions.png')
