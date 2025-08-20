using Microsoft.EntityFrameworkCore;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.repository
{
    public class ProductsRepository
    {
        private readonly ProductsDbContext dbContext;
        public ProductsRepository(ProductsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region create
        public async Task<Product> AddProduct(int id, ProductDto newProduct)
        {
            if (await GetProductById(id) != null)
                throw new BadHttpRequestException("Товар с ID " + id + " уже существует");
            var addedProduct = dbContext.Products.Add(new Product(newProduct) { Id = id });
            await dbContext.SaveChangesAsync();
            return addedProduct.Entity;
        }

        public async Task<Product> AddProduct(int id, ComboDto newCombo)
        {
            if (await GetProductById(id) != null)
                throw new BadHttpRequestException("Комбо с ID " + id + " уже существует");
            var addedCombo = dbContext.Products.Add(new Combo(newCombo) { Id = id });
            await dbContext.SaveChangesAsync();
            dbContext.ComboProducts.AddRange(newCombo.ProductsDict.Select(x=>new ComboProduct(addedCombo.Entity.Id, x.Key, x.Value)));
            return addedCombo.Entity;
        }
        #endregion

        #region read
        public async Task<List<Product>> GetProductsPage(int page, int size)
        {
            if (page < 1 || size < 1)
                throw new ArgumentException("Номер страницы и ее размер должны быть больше 0");
            page--;
            var foundedProducts=await dbContext.Products.OrderBy(p=>p.Id).Skip(page * size).Take(size).ToListAsync();
            if (foundedProducts.Count() == 0)
                throw new KeyNotFoundException("Страницы " + page + " не существует");
            return foundedProducts;
        }

        public async Task<Product> GetProductById(int id)
        {
            if(id<=0)
                throw new ArgumentException("Id продукта должно быть больше 0");
            var foundedProduct= await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (foundedProduct == null)
                throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
            return foundedProduct;
        }
        #endregion

        #region update
        public async Task<Product> UpdateProductInfo(int id, ProductDto updatedProduct)
        {
            var foundedProduct = await GetProductById(id);
            if (foundedProduct == null)
                throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
            dbContext.Entry(foundedProduct).CurrentValues.SetValues(updatedProduct);
            if (foundedProduct is Dish foundedDish && updatedProduct is DishDto updatedDish)
                UpdateDish(foundedDish, updatedDish);
            else if (foundedProduct is Combo foundedCombo && updatedProduct is ComboDto updatedCombo)
                UpdateCombo(foundedCombo, updatedCombo);
            await dbContext.SaveChangesAsync();
            return foundedProduct;
        }
        #endregion

        #region delete
        public async Task DeleteProductById(int id)
        {
            var foundedProduct = await GetProductById(id);
            if (foundedProduct == null)
                throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
            dbContext.Products.Remove(foundedProduct);
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region private
        private void DeleteComboProducts(Combo combo)
        {
            dbContext.ComboProducts.RemoveRange(combo.Products);
        }
        private void UpdateDish(Dish subject, DishDto dto)
        {
            subject.Proteins = dto.Proteins;
            subject.Fats = dto.Fats;
            subject.Carbohydrates = dto.Carbohydrates;
            subject.Calorific = dto.Calorific;
            subject.Ingredients = dto.Ingredients;
            subject.Allergens = dto.Allergens;
        }
        private void UpdateCombo(Combo subject, ComboDto dto)
        {
            if (dto.Products != null)
            {
                DeleteComboProducts(subject);
                dbContext.ComboProducts.AddRange(dto.ProductsDict.Select(x=> new ComboProduct(subject.Id, x.Key, x.Value)));
            }
        }
        #endregion
    }
}
