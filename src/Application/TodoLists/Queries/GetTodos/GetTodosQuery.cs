using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var lists = await _context.TodoLists
            .Include(l => l.Items)
            .AsNoTracking()
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);

        var listDtos = new List<TodoListDto>();

        foreach (var list in lists)
        {
            var dto = _mapper.Map<TodoListDto>(list);

            var allTags = list.Items
                .Where(i => i.Tags != null)
                .SelectMany(i => i.Tags!)
                .GroupBy(tag => tag)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            dto.TopTags = allTags;

            listDtos.Add(dto);
        }

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
                .ToList(),

            Lists = listDtos
        };
    }

}
