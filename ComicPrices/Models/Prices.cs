using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComicPrices.Models;

namespace ComicPrices.Models;

public class Price
{
    [Key]
    public int Id { get; set; }

    [Column("comic_id")]
    public int ComicId { get; set; }

    [Column("seller_id")]
    public int SellerId { get; set; }

    [Column("price")]
    public float Amount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("date_recorded")]
    public DateTime DateRecorded { get; set; }
    public Comic? Comic { get; set; }
    public Seller? Seller { get; set; }
}