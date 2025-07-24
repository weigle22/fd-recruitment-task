using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.UnitTests.TodoLists.Queries;

public class TodoItemDtoTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodoItem, TodoItemDto>()
                .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority))
                .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.Tags));
        });

        _mapper = config.CreateMapper();
    }

    [Test]
    public void Should_Map_TodoItem_To_TodoItemDto_Correctly()
    {
        // Arrange
        var entity = new TodoItem
        {
            Id = 10,
            ListId = 2,
            Title = "Write tests",
            Done = false,
            Priority = PriorityLevel.High,
            Note = "Add unit test for mapping",
            Colour = "#123abc",
            Tags = new List<string> { "test", "mapping" }
        };

        // Act
        var dto = _mapper.Map<TodoItemDto>(entity);

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.ListId.Should().Be(entity.ListId);
        dto.Title.Should().Be(entity.Title);
        dto.Done.Should().Be(entity.Done);
        dto.Priority.Should().Be((int)entity.Priority);
        dto.Note.Should().Be(entity.Note);
        dto.Colour.Should().Be(entity.Colour);
        dto.Tags.Should().BeEquivalentTo(entity.Tags);
    }
}
