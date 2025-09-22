using IdGen;
using Stock.Api.DTOs;
using Stock.Api.Model;
using Stock.Api.Repositories;
using Shared.Kernel.Core;

namespace Stock.Api.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IdGenerator _idGenerator;

        public ProductService(IProductRepository productRepository, IdGenerator idGenerator)
        {
            _productRepository = productRepository;
            _idGenerator = idGenerator; // <-- Adicionado
        }

        public async Task<Product> CreateProductAsync(CreateProductRequestDto request)
        {
            var newId = _idGenerator.CreateId();

            var product = new Product(
                newId,
                request.Sku,
                request.Name,
                request.Description,
                request.Category,
                request.Price,
                request.Quantity,
                request.ImageUrl
            );

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> GetByIdAsync(long id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            var products = await _productRepository.GetAllAsync();
            return products;
        }

        public async Task<StockValidationResultDto?> ValidateStockAsync(long id, int quantity, decimal price)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return null;
            }

            if (product.Price != price)
            {
                return new StockValidationResultDto
                {
                    IsValid = false,
                    Message = $"Preço do produto inválido. O preço correto é {product.Price}."
                };
            }

            if (product.Quantity < quantity)
            {
                return new StockValidationResultDto
                {
                    IsValid = false,
                    Message = $"Estoque insuficiente para o produto '{product.Name}'. Restam apenas {product.Quantity} unidades."
                };
            }

            return new StockValidationResultDto
            {
                IsValid = true,
                Message = "Estoque e preço validados com sucesso."
            };
        }


        public async Task<Result<Product>> UpdateProductAsync(long id, UpdateProductRequestDto request)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return Result<Product>.Failure("Produto não encontrado.");
            }

            product.Update(
                request.Name,
                request.Description,
                request.Category,
                request.Price,
                request.ImageUrl
            );

            await _productRepository.SaveChangesAsync();

            return Result<Product>.Success(product);
        }

        public async Task<Result<Product>> InactiveProduct(long id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return Result<Product>.Failure("Produto não encontrado.");
            }

            product.Inactivate(); 

            await _productRepository.SaveChangesAsync();

            return Result<Product>.Success(product);
        }
    }
}