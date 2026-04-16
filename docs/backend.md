# Backend Guidelines (.NET 8)

The backend ensures heavy lifting and scaling capabilities utilizing high-performance implementations of MediatR pattern.

## CQRS Workflow
1. **Controllers** expose the REST APIs, immediately invoking MediatR (`_mediator.Send()`).
2. **Commands**: For modifications. Place them under the respective `Domain/Application/Commands` directory. Commands perform database writes.
3. **Queries**: For read-only actions. Place them under the `Application/Queries` directory. Ensure queries access `IQueryable` from EF Core for maximum throughput without loading objects into memory unnecesarily.
4. **DTOs**: Ensure standard properties map correctly using `AutoMapper`. Add robust business-tier processing (like EMI calculations) into custom formatting Extensions.
