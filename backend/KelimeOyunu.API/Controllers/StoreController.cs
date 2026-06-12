using Microsoft.AspNetCore.Mvc;

namespace KelimeOyunu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStoreItems()
    {
        var items = new[]
        {
            new { Id = "gold_100", Name = "100 Altın", Description = "Küçük altın paketi", Price = "₺9.99", GoldAmount = 100, DiamondAmount = 0 },
            new { Id = "gold_500", Name = "500 Altın", Description = "Büyük altın paketi", Price = "₺39.99", GoldAmount = 500, DiamondAmount = 0 },
            new { Id = "gold_1000", Name = "1000 Altın", Description = "Mega altın paketi", Price = "₺69.99", GoldAmount = 1000, DiamondAmount = 0 },
            new { Id = "diamond_10", Name = "10 Elmas", Description = "Küçük elmas paketi", Price = "₺19.99", GoldAmount = 0, DiamondAmount = 10 },
            new { Id = "diamond_50", Name = "50 Elmas", Description = "Büyük elmas paketi", Price = "₺79.99", GoldAmount = 0, DiamondAmount = 50 },
            new { Id = "starter_pack", Name = "Başlangıç Paketi", Description = "300 Altın + 15 Elmas", Price = "₺29.99", GoldAmount = 300, DiamondAmount = 15 },
        };
        return Ok(items);
    }
}
