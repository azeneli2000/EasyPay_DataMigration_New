
# WorkOrders Data Migration – .NET API (ETL / Importer.Api)

  

A small ASP.NET Core API used to import legacy Excel data (clients, technicians, work orders)

into a SQL Server database.

  

Key capabilities:

  

 - Streaming Excel processing (no loading whole files into memory)
   
     
   
 - Batching using TVPs or SqlBulkCopy for high performance

   
     
   
   

 - Fuzzy matching for client names extracted from free-text notes

   
     
   
 

 - CSV reporting for both successful and failed imports

   
     
   
  

 - Database reporting for failed work orders

  

 **Important** – Before Running Importer.Api

  

You must run:

  

WorkOrders/WorkOrderManagement.API

  
  

This applies database migrations and creates the schema required for importing.

  

 **Architectural Patterns**

  

Clean Architecture / Onion Architecture

  

 Design Patterns Used

  

 1. Strategy Pattern

  

Used for switching DB execution mode (TVP / BulkCopy)

via keyed services configured in:

ImportConfigurations → WorkOrdersFromExcel → Mode

  

 2. Strategy via Higher-Order Functions

  

Mapping Excel rows is done by passing functions (mapRow) into the reader.

  

 3. Iterator Pattern

  

A custom async iterator streams Excel rows using IAsyncEnumerable.

  

 4. Producer–Consumer Pattern (Channels)

  

Producer: reads Excel rows and builds batches

  

Consumer: background task writing batches to DB + CSV report

  

# WorkOrders Management – .NET API (WorkOrders/WorkOrderManagement.API)

  

A simple ASP.NET Core API that provides CRUD operations for:

  

Work Orders

  

Clients

  

Technicians

  

**Architectural Pattern**

  

Vertical Slice Architecture

  

Each feature contains its own endpoint and logic.

  

 **Deployment Notes**

  

Both APIs (Importer.Api and WorkOrderManagement.API)

are independent microservices and can be deployed separately.