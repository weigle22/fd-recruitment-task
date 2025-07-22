using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.UnitTests.TodoItems.Commands;

public class UpdateTodoItemDetailCommandTests
{
    [Test]
    public async Task ShouldUpdateTodoItemDetail_Colour()
    {
        // Arrange
        var item = new TodoItem { Id = 2, Colour = "#000000" };

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(x => x.TodoItems.FindAsync(new object[] { 2 }, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(item);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        var handler = new UpdateTodoItemDetailCommandHandler(mockContext.Object);

        var command = new UpdateTodoItemDetailCommand
        {
            Id = 2,
            ListId = 1,
            Note = "Note",
            Priority = PriorityLevel.High,
            Colour = "#FFFFFF"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        item.Colour.Should().Be("#FFFFFF");
        mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
