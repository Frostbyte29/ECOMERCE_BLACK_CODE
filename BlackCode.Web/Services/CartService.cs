using System.Text.Json;
using BlackCode.Web.Data;
using BlackCode.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Services;

// Carrito guardado en la sesión del navegador. Los precios siempre se leen de la base de datos.
public class CartService(IHttpContextAccessor httpContextAccessor, BlackCodeContext context)
{
    private const string SessionKey = "BlackCode.Cart";
    public const int MaxQuantityPerLine = 10;
    public const decimal FreeShippingFrom = 150m;
    public const decimal ShippingCost = 10m;

    private ISession Session => httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("La sesión no está disponible.");

    public List<CartItem> GetItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return [];
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    public int Count() => GetItems().Sum(i => i.Quantity);

    public void Add(int productId, string? size, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (existing is null)
            items.Add(new CartItem { ProductId = productId, Size = size, Quantity = Clamp(quantity) });
        else
            existing.Quantity = Clamp(existing.Quantity + quantity);
        Save(items);
    }

    public void Update(int productId, string? size, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (existing is null) return;
        if (quantity <= 0) items.Remove(existing);
        else existing.Quantity = Clamp(quantity);
        Save(items);
    }

    public void Remove(int productId, string? size)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId && i.Size == size);
        Save(items);
    }

    public void Clear() => Session.Remove(SessionKey);

    public async Task<CartSummary> GetSummaryAsync()
    {
        var items = GetItems();
        var ids = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await context.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var lines = items
            .Where(i => products.ContainsKey(i.ProductId))
            .Select(i => new CartLine { Product = products[i.ProductId], Size = i.Size, Quantity = i.Quantity })
            .ToList();

        var subtotal = lines.Sum(l => l.LineTotal);
        return new CartSummary
        {
            Lines = lines,
            ShippingCost = lines.Count == 0 || subtotal >= FreeShippingFrom ? 0m : ShippingCost
        };
    }

    private static int Clamp(int quantity) => Math.Clamp(quantity, 1, MaxQuantityPerLine);

    private void Save(List<CartItem> items) => Session.SetString(SessionKey, JsonSerializer.Serialize(items));
}
