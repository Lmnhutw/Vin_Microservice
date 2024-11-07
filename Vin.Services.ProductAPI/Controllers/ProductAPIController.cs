using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vin.Services.ProductAPI.Data;
using Vin.Services.ProductAPI.Models;
using Vin.Services.ProductAPI.Models.DTO;

namespace Vin.Services.ProductAPI.Controllers
{
    [Route("api/Product")]
    [ApiController]

    public class ProductAPIController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ResponseDTO _res;
        private readonly IMapper _mapper;

        public ProductAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _res = new ResponseDTO();
        }

        [HttpGet]
        [Route("GetProductList")]
        public ResponseDTO Get()
        {
            try
            {
                IEnumerable<Product> prdList = _db.Products.ToList();
                _res.Result = _mapper.Map<IEnumerable<ProductDTO>>(prdList);
                _res.Message = "Get Product successfully";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = ex.Message;
            }
            return _res;
        }

        [HttpGet]
        [Route("GetProductById/{id:int}")]
        public ResponseDTO Get(int id)
        {
            try
            {
                Product prd = _db.Products.First(c => c.ProductId == id);
                /*ProductDTO prdDto = new ProductDTO()
                {
                    ProductId = prd.ProductId,
                    ProductCode = prd.ProductCode,
                    DiscountAmount = prd.DiscountAmount,
                    MinAmount = prd.MinAmount
                };*/
                _res.Result = _mapper.Map<Product>(prd);
                _res.Message = "Get Product successfully";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "There no Product ";
            }
            return _res;
        }


        [HttpPost]
        [Route("AddProduct")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Post([FromBody] ProductDTO productDTO)
        {
            try
            {
                // Check if the ProductCode is missing or empty
                /*if (string.IsNullOrWhiteSpace(productDTO.ProductId))
                {
                    _res.IsSuccess = false;
                    _res.Message = "Please enter the product code.";
                    return _res; // This will still return 200 OK with a failure message
                }*/

                // Map the DTO to the Product entity
                Product prd = _mapper.Map<Product>(productDTO);

                // Add the new product to the database
                _db.Products.Add(prd);
                _db.SaveChanges();

                // Return success response with the newly added product
                _res.Result = _mapper.Map<ProductDTO>(prd);
                _res.Message = "Product added successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the product: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }

        /*[HttpPut]
        [Route("UpdateProduct/")]
        public ResponseDTO Put([FromBody] ProductDTO productDTO, int id)
        {
            try
            {
                var prdExist = _db.Products.First(c => c.ProductId == id);
                // Check if the ProductCode is missing or empty
                if (prdExist is null)
                {
                    _res.IsSuccess = false;
                    _res.Message = "Product not found, Please enter another product!!";
                    return _res; // This will still return 200 OK with a failure message
                }

                // Ensure that the ProductCode is not empty
                if (string.IsNullOrWhiteSpace(productDTO.ProductCode))
                {
                    _res.IsSuccess = false;
                    _res.Message = "Please provide a valid Product Code!";
                    return _res;
                }
                //Map the properties from productDTO to prdExist(excluding the ProductId)
                _mapper.Map(productDTO, prdExist);
                // Ensure the ProductId remains unchanged by override the same ID
                //prdExist.ProductId = id;

                _db.Products.Update(prdExist);
                // Save the new product to the database
                _db.SaveChanges();

                // Return success response with the newly added product
                _res.Result = _mapper.Map<ProductDTO>(prdExist);
                _res.Message = "Product updated successfully.";
                _res.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the product: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }*/

        [HttpPut]
        [Route("UpdateProduct")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Put([FromBody] ProductDTO productDTO)
        {
            try
            {
                Product prdExist = _mapper.Map<Product>(productDTO);
                _db.Products.Update(prdExist);
                _db.SaveChanges();

                _res.Result = _mapper.Map<ProductDTO>(prdExist);
                _res.Message = "Product updated successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while adding the product: " + ex.Message;
            }

            return _res;
        }

        [HttpDelete]
        //[Route("DeleteProduct/{id:int}")]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Delete(int id)
        {
            try
            {
                var prdExist = _db.Products.FirstOrDefault(c => c.ProductId == id);
                // Check if the ProductCode is missing or empty
                if (prdExist is null)
                {
                    _res.IsSuccess = false;
                    _res.Message = "Product not found, Please enter another productId!!";
                    return _res; // This will still return 200 OK with a failure message
                }

                // Add the new product to the database
                _db.Products.Remove(prdExist);
                _db.SaveChanges();

                // Return success response with the newly added product
                //_res.Result = _mapper.Map<ProductDTO>(prdExist);
                _res.Message = "Product delete successfully.";
            }
            catch (Exception ex)
            {
                _res.IsSuccess = false;
                _res.Message = "An error occurred while delete the product: " + ex.Message;
            }

            return _res; // Always returns 200 OK, regardless of success or failure
        }
    }
}