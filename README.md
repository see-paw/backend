# Backend - SeePaw - Guia de Configuração

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- Visual Studio 2022 ou VS Code (opcional)

## Setup Inicial

### 1. Clonar o repositório
```bash
git clone <url-do-repositorio>
cd backend
```

### 2. Configurar variáveis de ambiente

Cria um ficheiro `.env` na pasta raiz do projeto com o seguinte conteúdo:
```bash
# .env
POSTGRES_DB=seepaw
POSTGRES_USER=seepaw
POSTGRES_PASSWORD=seepawpwd
```

> ⚠️ **Importante:** Este ficheiro **não** vai para o Git (está no `.gitignore`).  Normalmente estas credenciais nunca são enviadas para o repositório nem nunca são mostradas a ninguém,
> mas ficam aqui pois o projeto está enquadrado no contexto do curso.

### 3. Configurar User Secrets (desenvolvimento local)
```bash
cd API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=seepaw;Username=seepaw;Password=seepawpwd"
cd ..
```

### 4. Levantar a base de dados
```bash
docker-compose up -d database
```
> 💡 **O que isto faz:**  
> Cria e inicia um **container Docker** com PostgreSQL. O container é um ambiente isolado que corre a base de dados sem instalares PostgreSQL diretamente no teu PC. A flag `-d` (detached) corre em background, libertando o terminal.


Verificar que está saudável:
```bash
docker ps
```

Deves ver:
```
CONTAINER ID   IMAGE             STATUS
xxxxxx         postgres:latest   Up X seconds (healthy)
```

### 5. Correr a aplicação de backend
```bash
dotnet watch
```

Ou no Visual Studio: **F5** (seleciona o perfil **API**, não Docker Compose)

### 6. Testar

Abre o browser em: **https://localhost:5001/swagger**

Se vires a interface do Swagger, está tudo OK! ✅

## Workflow de Desenvolvimento

### Trabalhar com Migrations

#### Criar uma nova migration

Quando adicionares/modificares entidades novas:
```bash
# 1. Adiciona a entidade no Domain
# 2. Adiciona ao DbContext (Persistence/AppDbContext.cs)
# 3. Cria a migration
dotnet ef migrations add NomeDaMigration

# 4. Aplica a migration (ou arranca a app que aplica automaticamente)
dotnet ef database update
```

#### Atualizar a BD após pull

Se um colega adicionou migrations relevantes para o que estás a desenvolver:
```bash
git pull (ou merge a partir da respetiva branch)
cd API
dotnet watch
```

> 💡 A aplicação aplica migrations automaticamente ao arrancar!

```

## 🐳 Comandos Docker Úteis
```bash
# Ver containers a correr
docker ps

# Ver logs da BD
docker-compose logs database

# Parar a BD
docker-compose stop database

# Remover tudo (incluindo dados!)
docker-compose down -v

# Levantar apenas a BD
docker-compose up -d database
```

## Health Checks aos containers

### Base de Dados
```bash
# Verificar status
docker ps

# Testar conexão
docker exec -it database pg_isready -U seepaw -d seepaw
```

### API
```bash
# Deve mostrar as tabelas criadas
curl https://localhost:5001/swagger
```

## Estrutura do Projeto
```
backend/
├── .env                    # Variáveis Docker (não vai para Git)
├── docker-compose.yml      # Configuração Docker
├── API/                    # Camada de apresentação
├── Application/            # Lógica de negócio
├── Domain/                 # Entidades
└── Persistence/            # Acesso a dados
    └── Migrations/         # Histórico de mudanças na BD
```


## 📚 Recursos

- [Documentação .NET](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [Docker Compose](https://docs.docker.com/compose/)

---

**Nota:** Mantém sempre o Docker Desktop a correr quando desenvolveres!
