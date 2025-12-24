using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zumra.DTOs.Response;
using Zumra.IRepositories;
using Zumra.Models;
using Stripe.Checkout;
using System.Transactions;
using Zumra.Data;

namespace Zumra.Controllers;
[Authorize]
[Route("Order/[controller]/[action]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    // private readonly IBookRepository _bookRepository;

    public OrderController(
        ICartRepository cartRepository,
        UserManager<ApplicationUser> userManager,
        ICouponRepository couponRepository
        // IBookRepository bookRepository
        )
    {
        _cartRepository = cartRepository;
        _userManager = userManager;
        _couponRepository = couponRepository;
        // _bookRepository = bookRepository;
    }

    // ====== AddToCart ======
    // [HttpPost]
    // public async Task<IActionResult> AddToCart(int id)
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //
    //     var book = await _bookRepository.GetByIdAsync(id);
    //     if (book == null) return NotFound("Book not found");
    //
    //     if (book.Quantity <= 0)
    //         return BadRequest("This book is out of stock.");
    //
    //     
    //     var existingCart = await _cartRepository.GetByBookAndUserAsync(id, user.Id);
    //
    //     if (existingCart != null)
    //     {
    //         // تحقق من توفر الكمية المطلوبة قبل الزيادة
    //         if (book.Quantity < 1)
    //             return BadRequest("Not enough stock to add another item.");
    //
    //         existingCart.Quantity += 1;
    //         // لا تُخزن TotalPrice في الـ cart إن أمكن — حسابه مشتق
    //         existingCart.TotalPrice = existingCart.Quantity * book.Price;
    //
    //         book.Quantity -= 1;
    //
    //         await _bookRepository.UpdateAsync(book);
    //         await _cartRepository.UpdateAsync(existingCart);
    //     }
    //     else
    //     {
    //         var cartItem = new Cart
    //         {
    //             UserId = user.Id,
    //             BookId = id,
    //             Quantity = 1,
    //             TotalPrice = book.Price // اختياري — أو احذف الحقل واعتمد حسابًا مشتقًا
    //         };
    //
    //         book.Quantity -= 1;
    //         await _bookRepository.UpdateAsync(book);
    //         await _cartRepository.CreatAsync(cartItem);
    //     }
    //
    //     // إعادة حساب الإجمالي ديناميكيًا (لا تعتمد على حقل مخزن في المستخدم)
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var totalPrice = carts?.Sum(c => c.Quantity * c.Book.Price) ?? 0;
    //
    //     // إن أردت الاحتفاظ بالـ TotalCarts في المستخدم – حدّثه هنا، وإلا امسح السطور التالية
    //     user.TotalCarts = totalPrice;
    //     await _userManager.UpdateAsync(user);
    //
    //     return Ok(new
    //     {
    //         Message = "Added to cart",
    //         TotalCartPrice = totalPrice
    //     });
    // }
    //
    // // ====== GetAll ======
    // [HttpGet]
    // public async Task<IActionResult> GetAll()
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var items = (carts ?? Enumerable.Empty<Cart>())
    //         .Select(c => new CartResponse
    //         {
    //             Id = c.Id,
    //             Book = c.Book,
    //             Quantity = c.Quantity,
    //             TotalPrice = c.Quantity * c.Book.Price // حساب مشتق
    //         })
    //         .ToList();
    //
    //     var totalPrice = items.Sum(i => i.TotalPrice);
    //
    //     return Ok(new
    //     {
    //         Items = items,
    //         TotalPrice = totalPrice
    //     });
    // }
    //
    // // ====== UpdateCart ======
    // [HttpPost]
    // public async Task<IActionResult> UpdateCart(int Quantity, int id)
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //
    //     var cartUser = await _cartRepository.GetByIdAsync(id);
    //     if (cartUser == null) return NotFound("Cart item not found");
    //     if (cartUser.UserId != user.Id) return Forbid();
    //
    //     var book = await _bookRepository.GetByIdAsync(cartUser.BookId);
    //     if (book == null) return NotFound("Book not found");
    //
    //     if (Quantity <= 0)
    //         return BadRequest("Quantity must be at least 1.");
    //
    //     int oldQty = cartUser.Quantity;
    //
    //     if (Quantity > oldQty)
    //     {
    //         int diff = Quantity - oldQty;
    //         if (book.Quantity < diff)
    //             return BadRequest("Not enough stock.");
    //
    //         book.Quantity -= diff;
    //     }
    //     else if (Quantity < oldQty)
    //     {
    //         int diff = oldQty - Quantity;
    //         book.Quantity += diff;
    //     }
    //
    //     cartUser.Quantity = Quantity;
    //     cartUser.TotalPrice = cartUser.Quantity * book.Price;
    //
    //     await _bookRepository.UpdateAsync(book);
    //     await _cartRepository.UpdateAsync(cartUser);
    //
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var totalPrice = carts?.Sum(c => c.Quantity * c.Book.Price) ?? 0;
    //
    //     // حدّث المستخدم إن كنت تحفظ الإجمالي (اختياري)
    //     user.TotalCarts = totalPrice;
    //     await _userManager.UpdateAsync(user);
    //
    //     return Ok(new
    //     {
    //         Message = "Cart updated successfully",
    //         TotalCartPrice = totalPrice
    //     });
    // }
    //
    // // ====== RemoveFromCart ======
    // [HttpDelete]
    // public async Task<IActionResult> RemoveFromCart(int id)
    // {
    //     var cartUser = await _cartRepository.GetByIdAsync(id);
    //     if (cartUser == null) return NotFound("Cart item not found");
    //
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //     if (cartUser.UserId != user.Id) return Forbid();
    //
    //     var book = await _bookRepository.GetByIdAsync(cartUser.BookId);
    //     if (book != null)
    //     {
    //         book.Quantity += cartUser.Quantity;
    //         await _bookRepository.UpdateAsync(book);
    //     }
    //
    //     await _cartRepository.DeleteAsync(cartUser.Id);
    //
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var totalPrice = carts?.Sum(c => c.Quantity * c.Book.Price) ?? 0;
    //
    //     user.TotalCarts = totalPrice;
    //     await _userManager.UpdateAsync(user);
    //
    //     return Ok(new
    //     {
    //         Message = "Product deleted successfully",
    //         TotalCartPrice = totalPrice
    //     });
    // }
    //
    // // ====== ApplyCoupon ======
    // [HttpPost]
    // public async Task<IActionResult> ApplyCoupon(string code)
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var total = carts?.Sum(c => c.Quantity * c.Book.Price) ?? 0;
    //
    //     var cop = await _couponRepository.GetByCodeAsync(code);
    //     if (cop == null) return NotFound("Coupon not found");
    //
    //     var discount = (total * cop.DiscountAmount) / 100;
    //     if (discount > total) discount = total;
    //
    //     var newTotal = total - discount;
    //
    //     // الأفضل: اربط الكوبون بالسلة بدلاً من تغيير حقل المستخدم
    //     // يمكنك حفظ applied coupon على Cart أو على جدول Order مؤقت
    //     user.TotalCarts = newTotal; // اختياري — أو أعِد الإجمالي فقط في response
    //     await _userManager.UpdateAsync(user);
    //
    //     return Ok(new
    //     {
    //         Message = $"Coupon applied, saved {discount} EGP",
    //         TotalCartPrice = newTotal
    //     });
    // }
    //
    // // ====== Pay (Stripe) ======
    // [HttpPost]
    // public async Task<IActionResult> Pay()
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound("User not found");
    //
    //     var carts = await _cartRepository.GetByIdUserAsync(user.Id);
    //     var total = carts?.Sum(c => c.Quantity * c.Book.Price) ?? 0;
    //
    //     if (total <= 0) return BadRequest("Cart is empty");
    //
    //     // تأكد من إعداد مفتاح Stripe في إعدادات التطبيق
    //     // StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    //
    //     var lineItems = new List<SessionLineItemOptions>
    //     {
    //         new SessionLineItemOptions
    //         {
    //             PriceData = new SessionLineItemPriceDataOptions
    //             {
    //                 Currency = "egp",
    //                 ProductData = new SessionLineItemPriceDataProductDataOptions
    //                 {
    //                     Name = "Cart Total"
    //                 },
    //                 UnitAmount = (long)(total * 100) // EGP -> piasters
    //             },
    //             Quantity = 1
    //         }
    //     };
    //
    //     var options = new SessionCreateOptions
    //     {
    //         PaymentMethodTypes = new List<string> { "card" },
    //         LineItems = lineItems,
    //         Mode = "payment",
    //         SuccessUrl = $"{Request.Scheme}://{Request.Host}/Order/success",
    //         CancelUrl = $"{Request.Scheme}://{Request.Host}/Order/cancel"
    //     };
    //
    //     var service = new SessionService();
    //     var session = await service.CreateAsync(options);
    //
    //     if (string.IsNullOrEmpty(session.Url))
    //         return StatusCode(500, "Unable to create payment session");
    //
    //     return Ok(new { CheckoutUrl = session.Url });
    // }
    //
    // // ====== success ======
    // [HttpGet]
    // public async Task<IActionResult> success()
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null) return NotFound();
    //
    //     // عند نجاح الدفع: 
    //     // 1) يُفضّل إنشاء Order وتخزين تفاصيله
    //     // 2) حذف عناصر السلة
    //     // 3) لا تُعيد فقط حذف السلة دون تسجيل الطلب (لأغراض المحاسبة)
    //     await _cartRepository.DeleteByUserAsync(user.Id);
    //
    //     // حدث الـ TotalCarts لو كنت تحفظه
    //     user.TotalCarts = 0;
    //     await _userManager.UpdateAsync(user);
    //
    //     return Ok(new { Message = "Payment success and cart cleared" });
    // }
}
