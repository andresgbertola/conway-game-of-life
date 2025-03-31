# Conway's Game of Life API

## Installation

1. Download docker desktop from https://docs.docker.com/desktop/setup/install/windows-install/.
2. Check that Docker is running.
3. Checkout repository
4. Execute in the repository root folder (where docker-compose.yml is) the following command in PowerShell: 
``` 
    docker compose up -d
```
5. Once it finishes, check if the image and the containers were created in Docker.
6. You are ready to start enjoying the app :)

## How to
1. **Access SwaggerDocs**  
   Once the app is running, you can open [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html) in your browser to view and interact with all available endpoints.

2. **Use Swagger to Execute Endpoints**  
   From the Swagger UI, you can easily test each endpoint by providing the necessary parameters or request body.

3. **Create a New Board**  
   To create a new board, go to **api/boardState (POST)**. In the request body, provide the initial board configuration by listing all living cells in an array under `liveCells`. For example:

   ```json
   {
     "liveCells": [
       { "row": 0, "col": 1 },
       { "row": 1, "col": 1 },
       { "row": 2, "col": 0 },
       { "row": 2, "col": 1 },
       { "row": 2, "col": 2 },
       { "row": 3, "col": 1 },
       { "row": 4, "col": 1 },
       { "row": 5, "col": 1 },
       { "row": 6, "col": 1 }
     ]
   }
   ```

   Corresponds to the following pattern:

   ```
   ▫   ■   ▫
   ▫   ■   ▫
   ■   ■   ■
   ▫   ■   ▫
   ▫   ■   ▫
   ▫   ■   ▫
   ▫   ■   ▫
   ```

4. **Obtain and Use the Board ID**  
   After creating a new board, you'll receive a **Board ID** (GUID). You can:
   - **Retrieve the current board state** by sending a GET request to `api/boardState/{id}`.
   - **Advance the board** by sending a POST request to:
     - `api/boardState/{id}/next` (one generation),
     - `api/boardState/{id}/next/{steps}` (multiple generations),
     - or `api/boardState/{id}/final` (all the way to a final state).


## Problem Description

Conway's Game of Life is a cellular automaton devised by mathematician John Conway in 1970. It is a **zero-player game**, meaning its evolution is determined by its initial state and requires no further input.

The game takes place on an infinite two-dimensional grid of cells, where each cell can be either **alive** or **dead**.

## Rules

The game evolves through generations based on four simple rules:

1. **Underpopulation**: Any live cell with fewer than two live neighbors dies.
2. **Survival**: Any live cell with two or three live neighbors lives on to the next generation.
3. **Overpopulation**: Any live cell with more than three live neighbors dies.
4. **Reproduction**: Any dead cell with exactly three live neighbors becomes a live cell.

## Client Requirements and constraints

The following should be done in C# using net7.0 as the target framework.
Implement an API for Conway's Game of Life. Conway's Game of Life - Wikipedia
The API should have implementations for at least the following:
1. Allows uploading a new board state, returns id of board
2. Get next state for board, returns next state
3. Gets x number of states away for board
4. Gets final state for board. If board doesn't go to conclusion after x number of attempts, returns
error
The service you write should be able to restart/crash/etc... but retain the state of the boards.
The code you write should be production ready. You don’t need to implement any
authentication/authorization. Be prepared to show how your code meets production ready
requirements.

## API Key Features and Requirements and constraints

- **Board Management**: Create game boards by specifying living cells coordinates.
- **State Progression**: Advance the simulation by one step at a time, many steps at a time or run it until it finishes.
- **State Querying**: Retrieve the current state of the board at any point.

### Assumptions

- **Persistence of States**  
  All board states are saved in the database to:
  - Allow tracking of previous states.
  - Detect if the game has reached a final state.
  - Resume simulation after application restarts or failures.
- **Board States**  
  - `NotFinished`: The board is still evolving.
  - `FadedAway`: The board has no more living cells.
  - `Oscillatory`: The board has entered a repeating pattern.
  - `Stable`: The board has reached a configuration that no longer changes.
- **Final States**  
  The board is considered to have finished if its status is one of the following:
  - `FadedAway`
  - `Oscillatory`
  - `Stable`
- **Initial State**  
  When a new board is created, it starts in the `NotFinished` state.
- **Post-Final State Behavior**  
  Even after reaching a final state, further steps can be triggered. However, the board's status will not change.
- **Max Iteration Rule**  
  - When attempting to reach a final state with a `maxIterations` limit, if the final state is not reached within the limit, an error is thrown.
  - Clients may call the endpoint again to continue simulation from the last state.
  - This approach allows long-running generations to be processed incrementally and avoids blocking requests.
- **No limits**  
  - Creating a new board consists of sending the alive cells as a list of cell coordinates.
  - This approach allows the board to be dynamically sized based on active cells, meaning there is no predefined size limit — at least by design.
  - However, practical limitations may still apply depending on performance, storage, and system resources.

## API Solution Overview

This API provides a **RESTful interface** to simulate Conway's Game of Life. 
It follows **Clean Architecture** principles and applies **Domain-Driven Design (DDD)** to ensure a clear separation of concerns across well-defined layers. Additionally, it adopts the **CQRS (Command Query Responsibility Segregation)** pattern to distinguish between read and write operations. This approach helps to build modular, testable, and maintainable components by avoiding large service classes and favoring focused, single-responsibility handlers.

### Quick Summary
- **Board Creation**  
  A board is created by **POST**ing a list of living cells (`List<CellCoordinates>`) to the `api/boardState` endpoint.  
  When this call is made, a new **BoardState** record is saved in the database containing:
  - **BoardId**: Uniquely identifies the board
  - **Iteration**: The current generation count
  - **State**: A JSON-serialized list of living cells
  - **Hash**: A hash of the `State` to enable quick lookups
- **Advancing the Board**  
  Whenever an endpoint triggers moving the board to the next state, the following steps occur:
  1. Compute the next state based on the current board configuration.
  2. Check if the resulting state hash has already been stored for this board. If it has, the system knows the board has **finished** and updates the status.
  3. If the hash does not exists for that board, then it’s a new state, si save a new **BoardState** record in the database with the updated iteration and the new state’s hash.

### Tech Stack

- **.NET 7** for the API  
  Although .NET 7 is now obsolete, it was a specific requirement from the stakeholder.

- **MediatR** for handling queries and commands  
  Promotes the single-responsibility principle by separating logic into focused handlers, following the CQRS pattern.

- **AutoMapper** for object-to-object mapping  
  Simplifies transformation between domain models and DTOs.

- **Swagger** for API execution and Open Api documentation

- **EF Core** for data access  
  Used with SQL Server as the main database provider and with the InMemory provider for Integration Tests.

- **xUnit** for Unit and Integration Testing  
  Ensures correctness of business logic and API behavior.

- **SQL Server** as the persistence layer  
  Chosen for its scalability and robustness in managing structured data.

- **Docker** for containerized deployment  
  Allows the application to run consistently across environments. Facilitates testing without requiring local setup, and supports orchestration via Kubernetes or similar platforms in any cloud provider.

### Layered Architecture:

- **Domain**: Contains the core game logic, interfaces and entities that represent the business rules of Conway's Game of Life.
- **Application**: Implements use cases and coordinates business logic through commands and queries. Also includes DTOs, mappings between Domain entities and DTOs, Command/Queries Validations and custom application exceptions.
- **Infrastructure**: Manages external concerns such as persistence, EF Core DbContext, entity mappings, migrations and repository implementation.
- **WebApi**: Exposes HTTP endpoints for client interaction with the application. Thin layer that uses MediatR pattern (command/query) to handle the requests promoting a clean separation between the transport layer and the application logic.

This architecture promotes scalability, readability, and ease of testing by isolating each responsibility in its proper place.

### Components
#### WebApi
##### Controllers
- **BoardStateController**
  - `GET /api/boardstate/{id}` _(Idempotent)_  
    Retrieves the current board state by executing the `GetLastBoardStateByIdQuery` via MediatR. Returns the current `BoardStateDto`.

  - `POST /api/boardstate` _(Not Idempotent)_  
    Creates a new board by executing the `CreateNewBoardCommand` via MediatR using the list of live cells provided in the request body. Returns the ID of the newly created board.

  - `POST /api/boardstate/{id}/next` _(Not Idempotent)_  
    Advances the board to its next generation using the `UpdateBoardStatusCommand`. Returns the updated `BoardStateDto`.

  - `POST /api/boardstate/{id}/next/{steps}` _(Not Idempotent)_  
    Advances the board by a given number of steps by executing `UpdateBoardStatusCommand`. Returns the updated `BoardStateDto`.

  - `POST /api/boardstate/{id}/final?maxAttempts={n}` _(Not Idempotent)_  
    Tries to progress the board to a final state within a maximum number of iterations using `UpdateBoardStatusCommand`.  
    If the final state is not reached, a 422 error is returned. The client can call this endpoint again to continue from the last state.  
    Returns the current `BoardStateDto`.

##### Exception Handling, Logging & Validation
The API uses a centralized **middleware** to handle exceptions, ensuring:
- Sensitive details are never exposed to clients.
- Friendly and informative error messages are returned.
###### Custom Exceptions Handled:
- **ValidationException**  
  - Returns: `400 Bad Request`  
  - Includes: A list of validation errors to help the client correct the request.
- **NotFoundException**  
  - Returns: `404 Not Found`  
  - Used when the requested resource (e.g., board ID) does not exist.
- **CustomException**  
  - Returns: Custom HTTP status code (e.g., `422 Unprocessable Entity`)  
  - Used for domain-specific errors, such as when the board fails to reach a final state within the allowed steps.

All exceptions are **logged** with the appropriate log level:
- Unhandled exceptions are logged with **Error** level.
- Domain or validation-related issues may be logged with **Warning** or **Information** as needed.

#### GameOfLifeService Service Overview
Instead of iterating through an entire board (which could be vast), the service tracks only **living cells** and their **immediate neighbors**. It does this efficiently by:
- **Using a list of live cell coordinates** to represent the board state.
- **Counting neighbors** only for cells adjacent to current live cells, drastically reducing the number of calculations.
##### Step-by-Step Process
1. **Track Live Cells**:
   - The algorithm starts by accepting a list of coordinates that represent currently living cells.
2. **Identify Neighbor Cells**:
   - For each living cell, the algorithm identifies its **eight neighboring cells** (top, bottom, left, right, and diagonals).
3. **Count Neighbors**:
   - A count is maintained for each cell indicating how many live cells surround it.
   - Cells that appear multiple times indicate multiple neighboring live cells.
4. **Apply Game Rules**:
   - **Survival**: Any cell already alive remains alive only if it has **exactly 2 or 3 neighbors**.
   - **Birth**: A dead cell becomes alive if it has **exactly 3 neighbors**.
   - **Death**: All other cells either remain dead or die due to underpopulation or overpopulation.
5. **Generate Next Generation**:
   - After applying these rules, the algorithm compiles a new list of coordinates representing the live cells in the **next generation**.

##### Advantages of This Approach
- **Performance**: Only calculates cells adjacent to live cells, significantly optimizing computation.
- **Scalability**: Works efficiently even for large or sparse boards, as it avoids unnecessary iterations.
- **Memory Efficiency**: Avoids storing large matrices, only tracking cells relevant to each generation.

This efficient implementation makes the API suitable for large-scale simulations and ensures quick response times, making it ideal for interactive applications, educational tools, and real-time visualization.

#### Entity and Database Overview
##### BoardState Entity Overview

Board is not an entity itself as it was not required any extra info than the live cells.
That is why the API stores board states instead.
Each board generation will store one new `BoardState` in the database. In other words, `BoardState` is the domain entity representing a single state of a board within the application. 

###### Entity Details

| Property     | Description                                                             |
|--------------|-------------------------------------------------------------------------|
| **Id**       | Unique identifier of the specific board state record.                   |
| **BoardId**  | Identifier grouping multiple states of the same game board.             |
| **State**    | JSON representation of live cell coordinates (List<CellCoordinates>).   |
| **StateHash**| Computed hash (FNV-1a) of the board state for fast state comparison.|
| **Iteration**| Generation number indicating the step in the board evolution.           |
| **Status**   | Enum indicating if the board is `NotFinished`, `Stable`, `Oscillatory`, or has `FadedAway`. |

###### How `BoardState` Works:

- **List of live cells**:  
  Instead of storing the entire board matrix, the entity stores only coordinates of living cells. This approach is highly efficient for sparse boards and significantly reduces storage requirements.

- **Custom JSON Serializer for Cell Coordinates**:  
  To further minimize database storage, the entity employs a custom JSON serializer (`CellCoordinatesConverter`). Instead of storing each cell as verbose JSON objects (`{"Row":1,"Col":2}`), it stores cell coordinates as compact JSON arrays (`[1,2]`). This approach significantly reduces JSON size and thus decreases the overall database space consumption.

  **Example:**
  ```json
  // Before custom serializer:
  [{"Row":1,"Col":2},{"Row":3,"Col":4}]

  // After custom serializer:
  [[1,2],[3,4]]

- This approach reduces database size and speeds up reads/writes by only storing live cells in the most compact JSON format. And allows to quickly identifiy repeating board patterns using hashing, allowing for rapid state comparisons.

###### Database Indexes
To ensure optimal performance and scalability when querying and updating board states, the following indexes are used:
- **Primary Key: `Id` (Guid)**  
  A non-clustered primary key is used on the `Id` column.  
  This prevents page fragmentation and row reordering during inserts, as new states are frequently appended.
- **Clustered Index: `BoardId`, `Iteration`**  
  A clustered index is applied on the combination of `BoardId` and `Iteration`.  
  This supports efficient retrieval of the most recent state of a given board, since queries request the latest iteration (e.g., `ORDER BY Iteration DESC`).
- **Non-Clustered Index: `BoardId`, `Iteration`, `StateHash`**  
  This composite index accelerates lookups needed to determine if a board state has already occurred.  
  When computing the next generation, the system must check if the resulting state (identified by its hash) has been previously stored for the same board.  
  This check is part of a critical performance-sensitive loop, so fast access through indexing is essential.

### Concurrency / Consistency Check
- To ensure **data consistency** and avoid concurrent updates corrupting the board's progression, a **clustered unique index** is applied on the `BoardState` table using the combination of `BoardId` and `Iteration`.
- This guarantees that only one record per iteration per board can exist, preventing multiple requests from saving conflicting or duplicate states.
- As a result, if two parallel processes attempt to compute and persist the same iteration, only one will succeed, preserving the integrity of the board’s evolution, the other, will fail.

## Enhancements
- **Performance Optimization**
  - Implement caching in `UpdateBoardStatusCommandHandler` to reduce database queries when checking for existing state hashes.
    Use a HashSet or an in-memory dictionary to store previously seen hashes per board for quick lookups. Distributed cache not necessary in this case.
- **Input Validations**
  - Add constraints to limit the maximum board size to prevent performance degradation and excessive resource consumption.
- **Security**
  - Protect sensitive endpoints by implementing proper **API authentication**.
  - Use **Azure Key Vault** or any secure secret management tool to store sensitive data (e.g., DB passwords, API keys) instead of `appsettings.json`.
  - Avoid storing credentials in Dockerfiles by using **Docker Secrets**, environment variables, or secure secret injection at runtime.
- **Scalability**
  - Introduce asynchronous job processing or background workers to handle the computation of large or long-running boards without blocking the main request thread. This can improve responsiveness and stability.
- **Tests**
  - Add remaining Unit Tests, Integration Tests and stress testing.
- **Logging**
  - Currently, logging is performed via the console.
  - For better observability and centralized monitoring, it is recommended to integrate with a structured or unstructured logging platform such as **Datadog**, **New Relic**, **Azure Application Insights** or any other.
  - These platforms allow:
    - Advanced querying and filtering of logs.
    - Correlation of requests and performance metrics.
    - Alerts and dashboards for real-time monitoring.

## Tests:

### Unit Tests:
- Most of the classes has unit tests.
- The most important tests are in GameOfLifeService in which many known patterns are tested.
#### Game of Life Service - Test Coverage
- ✅ `SingleLiveCell_Dies`: A single live cell dies due to underpopulation.
- ✅ `BlinkerOscillator_TogglesState`: A vertical blinker toggles to a horizontal line.
- ✅ `BlockStable_RemainsUnchanged`: A 2x2 block remains unchanged (still life).
- ✅ `BeehiveStable_RemainsUnchanged`: A beehive pattern remains unchanged (still life).
- ✅ `Glider_MovesDiagonally`: A glider moves diagonally after one generation.
- ✅ `Cross_Expansion`: A cross pattern evolves correctly over 177 iterations.
- ✅ `FullyLiveBoard_CornersSurvive`: Only the corners survive from a fully live 3x3 board. 

### Integration Tests 

#### BoardStateController
1.	Create a new board and verify its initial state:
    •	Create a new board with a specific initial state.
    •	Retrieve the board state and verify it matches the expected initial state.
2.	Retrieve the next state of the board:
    •	Request the next state of the board and verify it matches the expected next state.
3.	Retrieve the state of the board after a specified number of iterations:
    •	Request the state of the board after 10 iterations and verify it matches the expected state.
4.	Attempt to reach the final state with a limited number of iterations:
    •	Attempt to reach the final state of the board with a maximum of 100 iterations and verify it fails with an UnprocessableEntity status code.
    •	Verify that the board's state has progressed by the expected number of iterations.
5.	Retrieve the final state of the board:
    •	Request the final state of the board with a maximum of 1000 iterations and verify it matches the expected final state.

## Functional tests:
