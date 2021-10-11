using System.Linq;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using EvilBaschdi.Testing;
using FluentAssertions;
using NSubstitute;
using RenamePlayMemoriesImportFolders.Core;
using RenamePlayMemoriesImportFolders.Internal;
using Xunit;

namespace RenamePlayMemoriesImportFolders.Tests.Internal
{
    public class GenerateNewPathTests
    {
        [Theory, NSubstituteOmitAutoPropertiesTrueAutoData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(GenerateNewPath).GetConstructors());
        }

        [Theory, NSubstituteOmitAutoPropertiesTrueAutoData]
        public void Constructor_ReturnsInterfaceName(GenerateNewPath sut)
        {
            sut.Should().BeAssignableTo<IGenerateNewPath>();
        }

        [Theory, NSubstituteOmitAutoPropertiesTrueAutoData]
        public void Methods_HaveNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(GenerateNewPath).GetMethods().Where(method => !method.IsAbstract));
        }

        [Theory, NSubstituteOmitAutoPropertiesTrueAutoData]
        public void ValueFor_ForProvidedPath_ReturnsNewPath(
            [Frozen] IAppSettings appSettings,
            GenerateNewPath sut)
        {
            // Arrange
            var initDirectory = "X:\\Temp\\PM";
            appSettings.InitialDirectory.Returns(initDirectory);

            // Act
            var result = sut.ValueFor("20.10.2099");

            // Assert
            result.Should().Be("X:\\Temp\\PM\\2099\\2099-10-20");
        }
    }
}