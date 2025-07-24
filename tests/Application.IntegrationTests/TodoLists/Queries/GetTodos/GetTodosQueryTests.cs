using FluentAssertions;
using NUnit.Framework;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Application.TodoLists.Commands.CreateTodoList;
using Todo_App.Application.TodoLists.Queries.GetTodos;

namespace Todo_App.Application.IntegrationTests.TodoLists.Queries.GetTodos;

using static Testing;

public class GetTodosQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnAllTodoListsWithTopTagsAndPriorityLevels()
    {
        // Arrange
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "Personal"
        });

        // Create items with tags
        var tagsSet1 = new List<string> { "home", "urgent", "home" }; // home appears twice
        var tagsSet2 = new List<string> { "urgent", "work", "work" }; // work appears twice

        await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Do laundry",
            Tags = tagsSet1
        });

        await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Finish report",
            Tags = tagsSet2
        });

        // Act
        var result = await SendAsync(new GetTodosQuery());

        // Assert
        result.Should().NotBeNull();
        result.PriorityLevels.Should().NotBeNullOrEmpty();
        result.PriorityLevels.Should().Contain(p => p.Name == "High");
        result.Lists.Should().HaveCountGreaterThan(0);

        var list = result.Lists.FirstOrDefault(l => l.Title == "Personal");
        list.Should().NotBeNull();

        list!.TopTags.Should().NotBeNull();
        list.TopTags.Should().Contain("home");
        list.TopTags.Should().Contain("work");
        list.TopTags.Should().Contain("urgent");

        // Ensure only top 5 tags are returned
        list.TopTags.Count.Should().BeLessOrEqualTo(5);
    }
}
