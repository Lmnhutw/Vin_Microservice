using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.Services.CouponAPI.Data;
using Vin.Services.CouponAPI.Models;
using Vin.Services.CouponAPI.Models.DTO;

namespace Vin.Services.CouponAPI.Controllers
{
    [Route("api/Coupon")]
    [ApiController]
    // [Authorize]
    public class CouponAPIController : Controller
    {
        private readonly AppDbContext _db;
        private ResponseDTO _res;
        private IMapper _mapper;

        public CouponAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _res = new ResponseDTO();
        }

        [HttpGet]
        [Route("GetCouponList")]
        public ResponseDTO Get()
        {
            try
            {
                IEnumerable<Coupon> cpnList = _db.Coupons.ToList();
                _res.Result = _mapper.Map<IEnumerable<CouponDTO>>(cpnList);
                _res.Message = "Get Coupon successfully";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = ex.Message;
            }
            return _res;
        }

        [HttpGet]
        [Route("GetCouponById/{id:int}")]
        public ResponseDTO Get(int id)
        {
            try
            {
                Coupon cpn = _db.Coupons.First(c => c.CouponId == id);
                /*CouponDTO cpDto = new CouponDTO()
                {
                    CouponId = cpn.CouponId,
                    CouponCode = cpn.CouponCode,
                    DiscountAmount = cpn.DiscountAmount,
                    MinAmount = cpn.MinAmount
                };*/
                _res.Result = _mapper.Map<Coupon>(cpn);
                _res.Message = "Get Coupon successfully";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "There no Coupon ";
            }
            return _res;
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public ResponseDTO GetByCode(string code)
        {
            try
            {
                Coupon? cpn = _db.Coupons.FirstOrDefault(c => EF.Functions.Like(c.CouponCode, code));
                if (cpn != null)
                {
                    _res.Message = "Get Coupon successfully";
                    _res.Result = _mapper.Map<Coupon>(cpn);
                    return _res;
                }

                _res.IsSuccess = false;
                _res.Message = "There no Coupon ";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "There no Coupon" + ex.Message;
            }
            return _res;
        }

        [HttpPost]
        [Route("AddCoupon")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Post([FromBody] CouponDTO couponDTO)
        {
            try
            {
                // Check if the CouponCode is missing or empty
                if (string.IsNullOrWhiteSpace(couponDTO.CouponCode))
                {
                    _res.IsSuccess = false;
                    _res.Message = "Please enter the coupon code.";
                    return _res; // This will still return 200 OK with a failure message
                }

                // Map the DTO to the Coupon entity
                Coupon cpn = _mapper.Map<Coupon>(couponDTO);

                // Add the new coupon to the database
                _db.Coupons.Add(cpn);
                _db.SaveChanges();

                // Return success response with the newly added coupon
                _res.Result = _mapper.Map<CouponDTO>(cpn);
                _res.Message = "Coupon added successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the coupon: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }

        /*[HttpPut]
        [Route("UpdateCoupon/")]
        public ResponseDTO Put([FromBody] CouponDTO couponDTO, int id)
        {
            try
            {
                var cpExist = _db.Coupons.First(c => c.CouponId == id);
                // Check if the CouponCode is missing or empty
                if (cpExist is null)
                {
                    _res.IsSuccess = false;
                    _res.Message = "Coupon not found, Please enter another coupon!!";
                    return _res; // This will still return 200 OK with a failure message
                }

                // Ensure that the CouponCode is not empty
                if (string.IsNullOrWhiteSpace(couponDTO.CouponCode))
                {
                    _res.IsSuccess = false;
                    _res.Message = "Please provide a valid Coupon Code!";
                    return _res;
                }
                //Map the properties from couponDTO to cpExist(excluding the CouponId)
                _mapper.Map(couponDTO, cpExist);
                // Ensure the CouponId remains unchanged by override the same ID
                //cpExist.CouponId = id;

                _db.Coupons.Update(cpExist);
                // Save the new coupon to the database
                _db.SaveChanges();

                // Return success response with the newly added coupon
                _res.Result = _mapper.Map<CouponDTO>(cpExist);
                _res.Message = "Coupon updated successfully.";
                _res.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the coupon: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }*/

        [HttpPut]
        [Route("UpdateCoupon")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Put([FromBody] CouponDTO couponDTO)
        {
            try
            {
                Coupon cpExist = _mapper.Map<Coupon>(couponDTO);
                _db.Coupons.Update(cpExist);
                _db.SaveChanges();

                _res.Result = _mapper.Map<CouponDTO>(cpExist);
                _res.Message = "Coupon updated successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the coupon: " + ex.Message;
            }

            return _res;
        }

        [HttpDelete]
        //[Route("DeleteCoupon/{id:int}")]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Delete(int id)
        {
            try
            {
                var cpExist = _db.Coupons.First(c => c.CouponId == id);
                // Check if the CouponCode is missing or empty
                if (cpExist is null)
                {
                    _res.IsSuccess = false;
                    _res.Message = "Coupon not found, Please enter another couponId!!";
                    return _res; // This will still return 200 OK with a failure message
                }

                // Add the new coupon to the database
                _db.Coupons.Remove(cpExist);
                _db.SaveChanges();

                // Return success response with the newly added coupon
                //_res.Result = _mapper.Map<CouponDTO>(cpExist);
                _res.Message = "Coupon delete successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the coupon: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }
    }
}