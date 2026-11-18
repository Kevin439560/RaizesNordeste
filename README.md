# Raízes do Nordeste — API

Microsserviço RESTful desenvolvido em **.NET 10 (C#)** para gerenciar operações multicanal, controle de estoque por unidade, programa de fidelidade com consentimento e anonimização de dados (**LGPD**), além de integração com gateway de pagamento desacoplado (*MOCK*).

---

## 1. Arquitetura da Solução

O projeto segue os princípios de *Domain-Driven Design (DDD)* simplificado usando arquitetura em camadas, estruturado em 4 projetos na Solution:
* **`RaizesNordeste.Domain`**: Entidades de negócio, regras de domínio, enums e lógica de validação isolada.
* **`RaizesNordeste.Infra`**: Contexto de persistência relacional (**PostgreSQL** via **Entity Framework Core 10**) e *Seed Data*.
* **`RaizesDoNordeste.Aplicacao`**: Serviços de aplicação (`ClienteService`, `PedidoService`, `AuditoriaService`), DTOs e orquestração de casos de uso.
* **`RaizesNordeste.Api`**: Controladores REST, autenticação **JWT (Bearer)**, RBAC por *Roles*, Middleware Global de erros (`ErroPadraoDto`) e documentação Swagger.

---

## 2. Diagramas do Sistema

### 2.1. Diagrama de Casos de Uso
<img width="1513" height="1210" alt="image" src="https://github.com/user-attachments/assets/1a43726f-1c6c-4679-81af-fb40ec1ef029" />

### 2.1. Diagrama Entidade-Relacionamento (DER)
<img width="1672" height="1236" alt="image" src="https://github.com/user-attachments/assets/a2c49c49-138a-43a8-9197-6479ab55ce63" />

### 2.3. Diagrama de Classes e Arquitetura em Camadas
<img width="905" height="1278" alt="image" src="https://github.com/user-attachments/assets/f14b3c47-5036-46a8-a4d8-37e23fa322a6" />

## 3. Pré-requisitos para executar
* .NET 10 instalado na máquina
* PostgreSQL (versão 17 de preferencia) instalado na máquina com a porta padrão 5432
* Ferramenta de testes de api (como postman ou insomnia)

## 4. Configuração e Execução
### 4.1 Clonar o repositório e configurar conexão
Abra o arquivo appsettings.json que está no módulo RaizesNordeste.API e configure a connection String do seu postgreSQL local com a sua senha e seu usuário
```markdown
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=raizes_nordeste;Username=postgres;Password=SUA_SENHA_AQUI"
  },
  "Jwt": {
    "Key": "ChavePadraoEmCasosEspecificos!@#"
  }
}
```
### 4.2 Aplicar as migrations do banco
Abra o console gerenciador de pacotes no visual studio com o projeto apontando para RaizesNordeste.Infra
```
Update-Database
```

Caso não esteja utilizando o visual studio, abra o terminal na pasta raiz da solução e execute este comando
```
dotnet ef database update --project RaizesNordeste.Infra --startup-project RaizesNordeste
```
### 4.3 Executando a API
Inicie a aplicação no visual studio com F5 ou clicando no botao "Play" verde escrito http/https

Caso não esteja utilizando o visual studio, abra no terminal na pasta raiz da solução e execute este comando
```
dotnet run --project RaizesNordeste
```
A api estará acessível em https://localhost:7251/swagger/ (ou na porta que for informada no terminal)

## 5. Como fazer testes rápidos
Muitos endpoints exigem autorização e roles gerenciais, para isso 2 usuarios de teste são persistidos no banco assim que executamos o update database, são estes:

```
Cliente: {"email": "cliente@raizes.com", "senha": "123456"}

Gerente: {"email": "gerente@raizes.com", "senha": "123456"}
```

Entao voce chamara o endpoint POST /api/v1/auth/login e enviar as credenciais de um dos usuários
<img width="1220" height="1297" alt="image" src="https://github.com/user-attachments/assets/14df9978-33ab-408f-a4fb-f443af5e349a" />
Com isso você obterá o token dele, e poderá utilizar para autorizar as chamadas para os demais endpoints
<img width="213" height="97" alt="image" src="https://github.com/user-attachments/assets/ec41021e-4fd1-4c36-842c-55c910be3d9b" />
Cole o Token aqui e clique em authorize
<img width="682" height="319" alt="image" src="https://github.com/user-attachments/assets/6ad5cbb8-1294-46ce-b782-92dd0041924d" />

Agora você poderá chamar os demais endpoints sem receber 401 não autorizado nem 403 proibido

Para testes mais completos a collection de testes do postman está disponível na pasta raíz da solução neste projeto
