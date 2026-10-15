using System;
namespace MaritimeLegend.Domain {
public static class GameFormulas {
    public static double CannonDamage(Catalog c,Ship ship,GameState state) {
        var p=c.balance.combat;
        return p.baseDamage+c.ships[ship.type].guns*p.damagePerGun+ship.level*p.damagePerShipLevel+state.skillLevels[c.balance.skills.gunnerySkill];
    }
    public static int MarketPrice(Catalog c,Product product,int quality,double specialty,double pressure,bool buy) {
        double reference=product.basePrice*c.market.qualityFactors[quality]*specialty*Math.Exp(pressure);
        double value=reference*(1+(buy?1:-1)*c.market.spread/2);
        return Math.Max(1,(int)(buy?Math.Ceiling(value):Math.Floor(value)));
    }
}
}
