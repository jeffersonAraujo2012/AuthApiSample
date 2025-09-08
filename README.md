# AuthApiSample
Este projeto não faz parte de curso algum e foi feito por iniciativa própria. Ele foi feito para estudar autenticação com JWT token e Identity em projetos ASP.NET. O método de estudo foi construir uma pequena e muito simples API com endpoints de registro, login e um para testar a autenticação. Após a construção eu passei a olhar, ponto a ponto, o que foi feito para redigir o texto explicativo abaixo. Decidi fazer isso para aprender os conceitos e entender de fato o que se está fazendo. Ressalta-se novamente que o projeto é extremamente básico e não deve ser usado em produção. Esse projeto pode ou não ser incrementado para prosseguir os estudos, pois ou os estudos continuarão aqui, ou continuarão em outros projetos.

## Step 1 - Instalação dos pacotes necessários
Após criar o projeto, é necessário importar os pacotes necessários. São eles:

- **Npgsql.EntityFrameworkCore.PostgreSQL**: Pacote necessário para usar o banco de dados PostgreSQL.
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore**: Esse pacote é 2 em 1. Na verdade ele instala o Identity e o EF Core. O Identity fornece as classes e mecanismos de autenticação e autorização, já o EF Core é ORM padrão da Microsoft e projetos ASP.NET.
- **Microsoft.EntityFrameworkCore.Tools**: São as ferramentas de desenvolvimento do EF Core, necessário para trabalhar com migrations. Você precisa desse pacote se quiser criar classes C# e mapea-las via EF Core para o banco (Code First). O oposto também é possível, ou seja, mapear o banco para classes. 
- **Microsoft.AspNetCore.Authentication.JwtBearer**: Pacote necessário para ativar o middleware de autenticação via JWT Bearer token.

## Step 2 - Preparação das classes base
Após instalar os pacotes é necessário preparar as classes básicas como a de contexto e usuários.

### IdentityUser
A classe IdentityUser é a padrão para usuário dos Identity. Em diversas situações, quando se quer extender o usuário padrão do IdentyUser, cria-se classes que herdam essa classe base, algo como:

```C#
namespace AuthApiSample.Database
{
    public class ApplicationUser : IdentityUser
    {
        public string AdicionalProp { get; set; }
    }
}
```
No entanto, para o meu estudo eu não precisei disto, então em respeito ao príncipio YAGNI (You Aren`t Gonna Need It) eu usei a classe base do Identity.

### ApplicationDbContext

```C#
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthApiSample.Database
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
```
Essa é classe de contexto padrão nesse projeto e herda de IdentityDbContext, que por sua vez herda indiretamente de DbContext. Essa classe fornecida pelo Identity irá implementar de cara todas as suas tabelas padrão após executar o primeiro migrate. Repare que a classe IdentityDbContext recebe um tipo genérico, neste caso é a classe base IdentityUser.

> Para quem tem curiosidade para saber porque o construtor precisa receber um DbContextOptions, saiba que é porque o DbContext precisa dessas informações para realizar a configuração de sua conexão com o banco. Nossa classe herda de IdentityDbContext, que por sua vez herda indiretamente de DbContext, desta forma ela recebe a configuração que é repassada para as classes base até DbContext. **Esse DbContextOptions vem da configuração em Program.cs, que veremos em breve.**

## Step 3 - Configuração do banco de dados

No arquivo appsettings.json adicionei a chave padrão de connection string (**ConnectionStrings**) e dentro dela adicionei a chave **DefaultConnection**, onde adicionei minha string de conexão local.

```JSON
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost:5432;Database=AuthApiSample;Username=postgres;Password=123456"
}
```

Em Program.cs, logo após a criação do builder, eu adicionei a classe de contexto com o método builder.Services.AddDbContext<T>. O método possui um tipo genérico que deve ser uma classe que herda de DbContext. Ela recebe como parametros um options que é nada verdade um DbContextOptionsBuilder. Por trás dos panos esse builder será usado para gerar um DbContextOptions que será enviado para o construtor da classe T. No nosso caso a única configuração necessária é a declaração de qual provedor de banco de dados vamos utilizar com a devida string de conexão.

```C#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

O método **builder.Configuration.GetConnectionString** é um método helper que acessa o appsettings exatamente na chave ConnectionStrings. Ele recebe a chave que você deseja acessar nesta seção, no nosso caso a chave **DefaultConnection**.

## Step 4 - Configuração do Identity
