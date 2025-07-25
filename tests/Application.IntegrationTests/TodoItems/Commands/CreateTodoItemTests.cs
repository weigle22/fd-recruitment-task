﻿using FluentAssertions;
using NUnit.Framework;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Application.TodoLists.Commands.CreateTodoList;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.IntegrationTests.TodoItems.Commands;

using static Testing;

public class CreateTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new CreateTodoItemCommand();

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateTodoItem()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var command = new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Tasks",
            Colour = "#ffcc00",
            Tags = new List<string> { "work", "urgent" }
        };

        var itemId = await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().NotBeNull();
        item!.ListId.Should().Be(command.ListId);
        item.Title.Should().Be(command.Title);
        item.Colour.Should().Be(command.Colour);
        item.Tags.Should().BeEquivalentTo(command.Tags);
        item.CreatedBy.Should().Be(userId);
        item.Created.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
        item.LastModifiedBy.Should().Be(userId);
        item.LastModified.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldAllowEmptyTagsAndColour()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "Empty Tags List"
        });

        var command = new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "No tags and colour"
            // Colour and Tags left null
        };

        var itemId = await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().NotBeNull();
        item!.Colour.Should().BeNull();
        item.Tags.Should().BeNull(); // or BeEmpty() depending on how you initialize it in the entity
    }

}
