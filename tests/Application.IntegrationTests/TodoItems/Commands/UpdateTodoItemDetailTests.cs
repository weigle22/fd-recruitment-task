using FluentAssertions;
using NUnit.Framework;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Todo_App.Application.TodoLists.Commands.CreateTodoList;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.IntegrationTests.TodoItems.Commands;

using static Testing;

public class UpdateTodoItemDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldUpdateTodoItemDetails()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "Test List"
        });

        var itemId = await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Initial Task",
            Colour = "#ffffff",
            Tags = new List<string> { "initial" }
        });

        var updateCommand = new UpdateTodoItemDetailCommand
        {
            Id = itemId,
            ListId = listId,
            Priority = PriorityLevel.High,
            Note = "Updated note content",
            Colour = "#ff0000",
            Tags = new List<string> { "urgent", "work" }
        };

        await SendAsync(updateCommand);

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().NotBeNull();
        item!.Priority.Should().Be(PriorityLevel.High);
        item.Note.Should().Be("Updated note content");
        item.Colour.Should().Be("#ff0000");
        item.Tags.Should().BeEquivalentTo(new List<string> { "urgent", "work" });
    }

    [Test]
    public async Task ShouldThrowNotFoundExceptionWhenItemDoesNotExist()
    {
        var command = new UpdateTodoItemDetailCommand
        {
            Id = 9999, // Non-existent ID
            ListId = 1,
            Priority = PriorityLevel.Medium,
            Note = "Doesn't matter",
            Colour = "#123456",
            Tags = new List<string> { "none" }
        };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }
}
