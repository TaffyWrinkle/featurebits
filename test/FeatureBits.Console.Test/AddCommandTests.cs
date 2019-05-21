﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Dotnet.FBit.Command;
using Dotnet.FBit.CommandOptions;
using FeatureBits.Core;
using FeatureBits.Data;
using FeatureBits.Data.EF;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FeatureBits.Console.Test
{
    public class AddCommandTests
    {
        [Fact]
        public void It_can_be_created()
        {
            var opts = new AddOptions();
            var repo = Substitute.For<IFeatureBitsRepo>();
            var it = new AddCommand(opts, repo);

            it.Should().NotBeNull();
        }

        [Fact]
        public void It_throws_if_the_options_are_null()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => { new AddCommand(null, null); };

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void It_throws_if_the_repo_is_null()
        {
            var opts = new AddOptions();
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => { new AddCommand(opts, null); };

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void It_should_run_and_create_a_FeatureBit_when_the_onOff_bit_is_false()
        {
            // Arrange
            var sb = new StringBuilder();
            SystemContext.ConsoleWriteLine = s => sb.Append(s);
            var bit = new CommandFeatureBitDefintion { Name = "foo" };
            var opts = new AddOptions { Name = "foo" };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(bit).Returns(Task.FromResult((IFeatureBitDefinition)bit));

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            result.Should().Be(0);
            sb.ToString().Should().Be("Feature bit added.");
        }

        [Fact]
        public void It_should_handle_OnOff_false()
        {
            // Arrange
            CommandFeatureBitDefintion def = null;
            var sb = new StringBuilder();
            SystemContext.ConsoleWriteLine = s => sb.Append(s);
            var bit = new CommandFeatureBitDefintion { Name = "foo", OnOff = false };
            var opts = new AddOptions { Name = "foo", OnOff = "false" };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(Arg.Any<CommandFeatureBitDefintion>()).Returns(Task.FromResult((IFeatureBitDefinition)bit)).AndDoes(x =>
            {
                def = x.Arg<CommandFeatureBitDefintion>();
            });

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            result.Should().Be(0);
            sb.ToString().Should().Be("Feature bit added.");
            def.Should().NotBeNull();
            def.OnOff.Should().BeFalse();
        }

        [Fact]
        public void It_should_write_errors_to_the_console_if_theres_an_error()
        {
            // Arrange
            var sb = new StringBuilder();
            SystemContext.ConsoleErrorWriteLine = s => sb.Append(s);
            var opts = new AddOptions { Name = "foo" };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(Arg.Any<IFeatureBitDefinition>()).Returns<Task<IFeatureBitDefinition>>(x => throw new Exception("Yow!"));

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            result.Should().Be(1);
            sb.ToString().Should().StartWith("System.Exception: Yow!");
        }

        [Fact]
        public async Task It_can_BuildBit_that_is_valid()
        {
            // Arrange
            DateTime expectedDateTime = new DateTime(1966, 11, 9);
            SystemContext.Now = () => expectedDateTime;
            string expectedUsername = "bar";
            SystemContext.GetEnvironmentVariable = s => expectedUsername;
            var sb = new StringBuilder();
            SystemContext.ConsoleErrorWriteLine = s => sb.Append(s);
            var opts = new AddOptions
            {
                Name = "foo",
                OnOff = "1",
                ExcludedEnvironments = "QA,Production",
                MinimumPermissionLevel = 20
            };
            var repo = Substitute.For<IFeatureBitsRepo>();

            var it = new AddCommand(opts, repo);

            // Act
            var result = await it.BuildBit();

            // Assert
            result.CreatedDateTime.Should().Be(expectedDateTime);
            result.LastModifiedDateTime.Should().Be(expectedDateTime);
            result.CreatedByUser.Should().Be(expectedUsername);
            result.LastModifiedByUser.Should().Be(expectedUsername);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(result, new ValidationContext(result), validationResults, true);
            validationResults.Count.Should().Be(0);
        }

        [Fact]
        public void It_should_run_and_throw_by_default_if_the_featurebit_name_already_exists()
        {
            // Arrange
            var sb = new StringBuilder();
            SystemContext.ConsoleErrorWriteLine = s => sb.Append(s);
            var opts = new AddOptions { Name = "foo" };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(Arg.Any<IFeatureBitDefinition>()).Returns<Task<IFeatureBitDefinition>>(x => throw new FeatureBitDataException("Cannot add. Feature bit with name 'foo' already exists."));

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            result.Should().Be(1);
            sb.ToString().Should().Be("Feature bit 'foo' already exists. Use --force to overwrite existing feature bits.");
        }

        [Fact]
        public void It_should_run_and_throw_by_default_if_some_other_DataException_occurs_on_add()
        {
            // Arrange
            var sb = new StringBuilder();
            SystemContext.ConsoleErrorWriteLine = s => sb.Append(s);
            var opts = new AddOptions { Name = "foo" };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(Arg.Any<IFeatureBitDefinition>()).Returns<Task<IFeatureBitDefinition>>(x => throw new DataException("Some random DataException."));

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            result.Should().Be(1);
            sb.ToString().Should().StartWith("System.Data.DataException: Some random DataException.");
        }

        [Fact]
        public void It_should_run_and_update_a_FeatureBit_if_it_already_exists_and_force_is_specified()
        {
            // Arrange
            var sb = new StringBuilder();
            SystemContext.ConsoleWriteLine = s => sb.Append(s);
            var opts = new AddOptions { Name = "foo", Force = true };
            var repo = Substitute.For<IFeatureBitsRepo>();
            repo.AddAsync(Arg.Any<IFeatureBitDefinition>()).Returns<Task<IFeatureBitDefinition>>(x => throw new FeatureBitDataException("Cannot add. Feature bit with name 'foo' already exists."));

            int counter = 0;
            repo.When(x => x.UpdateAsync(Arg.Any<IFeatureBitDefinition>())).Do((x => counter++));

            var it = new AddCommand(opts, repo);

            // Act
            var result = it.RunAsync().Result;

            // Assert
            sb.ToString().Should().Be("Feature bit updated.");
            result.Should().Be(0);
            counter.Should().Be(1);
        }

        [Theory]
        [InlineData("foo", "test5", 0, "Feature bit added.")]
        [InlineData("foo", "test5,test7", 1, "Feature bit 'foo' has an invalid dependency [test5,test7].")]
        [InlineData("foo", "test5,7", 1, "Feature bit 'foo' has an invalid dependency [test5,7].")]
        [InlineData("foo", "test7", 1, "Feature bit 'foo' has a recursive loop [test7].", true)]
        public async Task It_should_run_FeatureBit_add_and_validate_dependencies(string featureBit, string dependentCsv, int expectedResult, string expectedMessage, bool recursive = false)
        {
            var initialSet = new List<FeatureBitEfDefinition>
            {
                new FeatureBitEfDefinition { Name = "test1"},
                new FeatureBitEfDefinition { Name = "test2"},
                new FeatureBitEfDefinition { Name = "test3", Dependencies = "test2,test1" },
                new FeatureBitEfDefinition { Name = "test4"},
                new FeatureBitEfDefinition { Name = "test5", Dependencies = "test3" },
            };
            if (recursive)
            {
                initialSet.AddRange(
                    new FeatureBitEfDefinition[] {
                        new FeatureBitEfDefinition { Name = "test6", Dependencies = "test7" },
                        new FeatureBitEfDefinition { Name = "test7", Dependencies = "test6"}
                    }
                );
            }

            var options = FeatureBitEfHelper.GetFakeDbOptions(true);
            using (var context = FeatureBitEfHelper.GetFakeDbContext(options))
            {
                context.FeatureBitDefinitions.AddRange(initialSet);
                var dbresults = context.SaveChanges();
                System.Diagnostics.Trace.TraceInformation($"Records {dbresults} persisted");
            }

            using (var context = FeatureBitEfHelper.GetFakeDbContext(options))
            {
                var repo = new FeatureBitsEfRepo(context);

                // Arrange
                var sb = new StringBuilder();
                SystemContext.ConsoleWriteLine = s => sb.Append(s);
                SystemContext.ConsoleErrorWriteLine = s => sb.Append(s);
                var opts = new AddOptions { Name = featureBit, Dependents = dependentCsv };
                var it = new AddCommand(opts, repo);

                var results = await it.RunAsync();
                results.Should().Be(expectedResult);

                var entities = await repo.GetAllAsync();
                sb.ToString().Should().Be(expectedMessage);
                if (expectedResult == 0)
                {
                    entities?.Count().Should().BeGreaterThan(initialSet.Count());
                }
                else
                {
                    entities?.Count().Should().Be(initialSet.Count());
                }
            }
        }
    }
}
