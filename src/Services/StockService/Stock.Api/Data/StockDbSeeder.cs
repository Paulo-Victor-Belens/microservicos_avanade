using IdGen;
using Stock.Api.Model;

namespace Stock.Api.Data
{
    public static class StockDbSeeder
    {
        public static void Seed(StockDbContext context, IdGenerator idGenerator)
        {
            if (context.Products.Any())
            {
                return;
            }

            var products = new List<Product>
            {
                new Product(
                    id: idGenerator.CreateId(),
                    sku: "BK-CL-001",
                    name: "Livro Arquitetura Limpa",
                    description: "Um guia do artesão para estrutura e design de software.",
                    category: "Livros",
                    price: 79.90m,
                    quantity: 50
                ),
                new Product(
                    id: idGenerator.CreateId(),
                    sku: "LP-DELL-XPS",
                    name: "Notebook XPS 15",
                    description: "Notebook de alta performance para desenvolvedores.",
                    category: "Eletrônicos",
                    price: 12500.00m,
                    quantity: 15
                ),
                new Product(
                    id: idGenerator.CreateId(),
                    sku: "KB-LOGI-MX",
                    name: "Teclado Mecânico MX Keys",
                    description: "Teclado sem fio com iluminação inteligente.",
                    category: "Acessórios",
                    price: 650.50m,
                    quantity: 30
                )
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}