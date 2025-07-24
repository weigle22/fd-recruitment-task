using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;
using Todo_App.Domain.ValueObjects;

namespace Todo_App.Application.UnitTests.TodoLists.Queries;

public class TodoListDtoTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Needed mapping for nested TodoItemDto
            cfg.CreateMap<TodoItem, TodoItemDto>()
                .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority))
                .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.Tags));

            // Mapping under test
            cfg.CreateMap<TodoList, TodoListDto>();
        });

        _mapper = config.CreateMapper();
    }

    [Test]
    public void Should_Map_TodoList_To_TodoListDto_Correctly()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 10,
            ListId = 1,
            Title = "Prepare slides",
            Done = false,
            Priority = PriorityLevel.Medium,
            Note = "For client meeting",
            Colour = Colour.Blue,
            Tags = new List<string> { "presentation", "work" }
        };

        var todoList = new TodoList
        {
            Id = 1,
            Title = "Work",
            Colour = Colour.Blue
        };

        // Using reflection to inject items for testing
        typeof(TodoList)
            .GetField("<Items>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(todoList, new List<TodoItem> { item });


        // Act
        var dto = _mapper.Map<TodoListDto>(todoList);

        // Assert
        dto.Id.Should().Be(todoList.Id);
        dto.Title.Should().Be(todoList.Title);
        dto.Colour.Should().Be(todoList.Colour.Code);

        dto.Items.Should().HaveCount(1);
        dto.Items[0].Title.Should().Be("Prepare slides");
        dto.Items[0].Priority.Should().Be((int)PriorityLevel.Medium);
        dto.Items[0].Tags.Should().BeEquivalentTo(new List<string> { "presentation", "work" });

        // TopTags should be empty as it's set outside mapping
        dto.TopTags.Should().BeEmpty();
    }
}
