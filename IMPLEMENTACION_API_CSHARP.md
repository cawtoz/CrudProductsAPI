# Guía de Implementación: API RESTful de Productos en C# con ASP.NET Core

## 📋 Tabla de Contenidos
- [Introducción](#introducción)
- [Requisitos Previos](#requisitos-previos)
- [Arquitectura y Comparación](#arquitectura-y-comparación)
- [Configuración del Proyecto](#configuración-del-proyecto)
- [Conceptos Clave de la Implementación](#conceptos-clave-de-la-implementación)
- [Ejecución](#ejecución)
- [Diferencias Principales Node.js vs C#](#diferencias-principales-nodejs-vs-c)

---

## 🎯 Introducción

Este documento explica cómo replicar la API RESTful de gestión de productos desarrollada en **Node.js/TypeScript con Express y TypeORM**, utilizando **C# con ASP.NET Core 8** y **Entity Framework Core**.

La API implementa las mismas operaciones CRUD (Create, Read, Delete) sobre productos almacenados en PostgreSQL, manteniendo la misma estructura de endpoints y respuestas.

---

## 📦 Requisitos Previos

- **.NET 8 SDK** instalado
- **PostgreSQL** (la misma base de datos usada en la versión Node.js)
- Editor de código (Visual Studio, VS Code, Rider)

---

## 🏗️ Arquitectura y Comparación

### Estructura del Proyecto C#

```
CrudProductsAPI/
├── Controllers/    # Endpoints de la API
│   └── ProductsController.cs
├── Models/   # Entidades del dominio
│ └── Product.cs
├── Data/      # Configuración de base de datos
│   └── ApplicationDbContext.cs
├── DTOs/            # Validaciones y respuestas
│   ├── CreateProductDto.cs
│   └── ErrorResponse.cs
├── Program.cs           # Configuración principal
└── appsettings.json     # Cadena de conexión
```

### Comparación con Node.js/TypeScript

| Concepto | Node.js/Express | C# ASP.NET Core | Explicación |
|----------|----------------|-----------------|-------------|
| **ORM** | TypeORM | Entity Framework Core | Ambos mapean objetos a tablas |
| **Decoradores** | `@Entity`, `@Column` | `[Table]`, `[Column]` | Atributos vs decoradores |
| **Validaciones** | Funciones en `validators.ts` | Data Annotations | C# valida automáticamente |
| **DI** | Manual | Nativo con `builder.Services` | .NET tiene DI integrado |
| **Enrutamiento** | `router.get('/products')` | `[HttpGet]` sobre métodos | Basado en atributos |
| **Respuestas** | `res.status(200).json(data)` | `return Ok(data)` | Helpers nativos |

---

## ⚙️ Configuración del Proyecto

### 1. Crear el proyecto

```bash
dotnet new webapi -n CrudProductsAPI
cd CrudProductsAPI
```

### 2. Instalar paquetes necesarios

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**Equivalencias:**
- `npm install typeorm pg` → **Npgsql.EntityFrameworkCore.PostgreSQL**
- No hay `package.json` → Se usa archivo `.csproj`

---

## 🔨 Conceptos Clave de la Implementación

### 1 Modelo de Datos (Equivalente a Entity de TypeORM)

**C# - `Models/Product.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name", TypeName = "varchar(255)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("description", TypeName = "text")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
}

```

**Comparación con TypeORM:**
- `@Entity('products')` → `[Table("products")]`
- `@PrimaryGeneratedColumn()` → `[Key]` con auto-increment
- `@Column({ type: 'decimal' })` → `[Column(TypeName = "decimal(10,2)")]`

---

### 2 DbContext (Equivalente a DataSource de TypeORM)

**C# - `Data/ApplicationDbContext.cs`:**
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
    {

    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}
```

**Configuración en `Program.cs`:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Comparación con TypeORM:**
```typescript
// Node.js
const AppDataSource = new DataSource({
  type: 'postgres',
  host: 'localhost',
  // ...
  entities: [Product]
});
```

En C#, la configuración se hace mediante **inyección de dependencias** y la cadena de conexión va en `appsettings.json`.

---

### 3 Validaciones con Data Annotations

**C# - `DTOs/CreateProductDto.cs`:**
```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "El campo 'name' es requerido")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 255 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo 'description' es requerido")]
    [MinLength(1, ErrorMessage = "La descripción no puede estar vacía")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo 'price' es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser un número positivo mayor a 0")]
    public decimal Price { get; set; }
}
```

**Ventaja sobre Node.js:**
- En Node.js validabas manualmente con funciones en `validators.ts`
- En C# el framework valida automáticamente y rellena `ModelState.IsValid`
- No necesitas escribir `if (!name || name.trim() === '')` manualmente

---

### 4 Controlador con Endpoints

**C# - `Controllers/ProductsController.cs`** (versión simplificada):

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {

        try
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los productos");
            return StatusCode(500, new ErrorResponse { Message = "Error al obtener los productos" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto productDto)
    {
        try
        {
            if (productDto == null)
            {
                return BadRequest(new ErrorResponse { Message = "Debes enviar información en el cuerpo de la solicitud" });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponse { Message = errors });
            }

            var product = new Product
            {
                Name = productDto.Name.Trim(),
                Description = productDto.Description.Trim(),
                Price = productDto.Price
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return StatusCode(201, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el producto");
            return StatusCode(500, new ErrorResponse { Message = "Error al crear el producto" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<object>> DeleteProduct(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse { Message = "El ID debe ser un número positivo" });
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new ErrorResponse { Message = "Producto no encontrado" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el producto");
            return StatusCode(500, new ErrorResponse { Message = "Error al eliminar el producto" });
        }
    }
}

```

**Comparación con Express:**

| Node.js/Express | C# ASP.NET |
|----------------|------------|
| `router.get('/products', getAllProducts)` | `[HttpGet]` sobre el método |
| `res.status(200).json(products)` | `return Ok(products)` |
| `res.status(404).json({ message })` | `return NotFound(new { message })` |
| `productRepository.find()` | `_context.Products.ToListAsync()` |

---

### 5 Configuración Principal

**`appsettings.json`** (cadena de conexión):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=products_db;Username=postgres;Password=postgres"
  }
}
```

**`Program.cs`** (configuración mínima):
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
   policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();
app.Run();
```

**Equivalente en Node.js:**
```typescript
const app = express();
app.use(cors());
app.use(express.json());
app.use('/api', productRoutes);
await AppDataSource.initialize();
app.listen(3000);
```

---

## 🚀 Ejecución


```bash
dotnet run
```

La API estará en:
- `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

---

## 🔄 Diferencias Principales Node.js vs C#

### Ventajas de C# para esta API

1. **Tipado estático nativo**: No necesitas TypeScript adicional
2. **Validaciones automáticas**: Data Annotations vs validación manual
3. **Inyección de dependencias**: Sistema robusto integrado
4. **Swagger automático**: Documentación generada sin configuración
5. **Performance**: Código compilado vs interpretado

### Conceptos que cambian

| Aspecto | Node.js | C# |
|---------|---------|-----|
| **Async/Await** | `async/await` con Promises | `async Task<T>` con Tasks |
| **Gestión de paquetes** | npm/yarn | NuGet (dotnet CLI) |
| **Archivos de config** | `.env`, `package.json` | `appsettings.json`, `.csproj` |
| **Middlewares** | `app.use()` | `app.UseMiddleware()` |
| **Logging** | `console.log()` | `ILogger<T>` inyectado |

### Lo que se mantiene igual

- ✅ Misma base de datos (PostgreSQL)
- ✅ Mismos endpoints y respuestas
- ✅ Misma lógica de negocio
- ✅ Mismas validaciones
- ✅ Mismo formato JSON

---

## ✅ Conclusión

La API en C# replica **fielmente** la funcionalidad de Node.js manteniendo:
- Los mismos endpoints
- Las mismas validaciones
- Los mismos códigos de estado HTTP
- El mismo formato de respuestas

**ASP.NET Core** ofrece un framework completo con herramientas integradas que simplifican el desarrollo: inyección de dependencias nativa, validación automática, Swagger incluido, y un sistema de tipos robusto sin necesidad de herramientas adicionales como TypeScript.

La principal diferencia conceptual es que **Node.js es más modular** (eliges cada librería), mientras que **.NET incluye todo lo necesario de forma estándar.

---

