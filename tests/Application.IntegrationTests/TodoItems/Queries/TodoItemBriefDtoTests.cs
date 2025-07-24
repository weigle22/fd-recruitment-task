using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using Todo_App.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.UnitTests.TodoItems.Queries;

public class TodoItemBriefDtoTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Manually create the map as in the Mapping method
            cfg.CreateMap<TodoItem, TodoItemBriefDto>()
                .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.Tags));
        });

        _mapper = config.CreateMapper();
    }

    [Test]
    public void Should_Map_TodoItem_To_TodoItemBriefDto_Correctly()
    {
        // Arrange
        var entity = new TodoItem
        {
            Id = 1,
            ListId = 2,
            Title = "Sample Task",
            Done = true,
            Colour = "#00ff00",
            Tags = new List<string> { "green", "done" }
        };

        // Act
        var dto = _mapper.Map<TodoItemBriefDto>(entity);

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.ListId.Should().Be(entity.ListId);
        dto.Title.Should().Be(entity.Title);
        dto.Done.Should().Be(entity.Done);
        dto.Colour.Should().Be(entity.Colour);
        dto.Tags.Should().BeEquivalentTo(entity.Tags);
    }
}
