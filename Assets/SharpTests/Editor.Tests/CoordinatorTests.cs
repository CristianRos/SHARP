using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using R3;
using SHARP.Core;
using SHARP.Tests.Utils;
using UnityEngine;


public class CoordinatorTests
{
    #region Constants

    const string DEFAULT_CONTEXT = "test_context";
    const string CONTEXT_1 = "test_context_1";
    const string CONTEXT_2 = "test_context_2";
    const string SHARED_CONTEXT = "shared_context";
    const string EXISTING_CONTEXT = "existing_context";

    #endregion

    #region Fields

    IContainer _container;
    Coordinator<ITestViewModel> _coordinator;

    ITestViewModel _viewModel1;
    ITestViewModel _viewModel2;
    IView<ITestViewModel> _view1;
    IView<ITestViewModel> _view2;

    #endregion

    #region Setup and TearDown

    [SetUp]
    public void Setup()
    {
        _container = Substitute.For<IContainer>();
        _coordinator = new();

        _viewModel1 = new TestViewModel();
        _viewModel2 = new TestViewModel();

        _view1 = Substitute.For<IView<ITestViewModel>>();
        _view2 = Substitute.For<IView<ITestViewModel>>();

        _view1.ViewModel.Returns(new ReactiveProperty<ITestViewModel>(default));
        _view2.ViewModel.Returns(new ReactiveProperty<ITestViewModel>(default));

        _container
            .Resolve<ITestViewModel>()
            .Returns(_viewModel1, _viewModel2);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _container?.Dispose();
            _coordinator?.Dispose();

            _coordinator?.AssertEmptyState();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception thrown during teardown: {ex}");
        }
    }

    #endregion


    #region View Creation

    [Test]
    public void ViewCreation_WithoutContext_CreatesUniqueViewModel()
    {
        // Arrange

        // Act
        _view1.InitView(_coordinator, _container);

        // Assert
        Assert.That(_view1.ViewModel.Value, Is.EqualTo(_viewModel1),
            "Should return the ViewModel from the container");

        _coordinator.AssertSingleActive(_viewModel1);
        _coordinator.AssertStateConsistent();

        _container.Received(1).Resolve<ITestViewModel>();
    }

    [Test]
    public void ViewCreation_WithSameContext_ReusesViewModel()
    {
        // Arrange

        const string testContext = SHARED_CONTEXT;
        _view1.Context = testContext;
        _view2.Context = testContext;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Assert
        Assert.That(_view2.ViewModel.Value, Is.SameAs(_view1.ViewModel.Value));

        _coordinator.AssertContextExists(testContext);
        _coordinator.AssertSingleActive(_viewModel1);
        _coordinator.AssertStateConsistent();

        _coordinator.AssertViewsSharingContext(testContext, _view1, _view2);

        _container.Received(1).Resolve<ITestViewModel>();
    }

    [Test]
    public void ViewCreation_WithDifferentContexts_CreatesUniqueViewModels()
    {
        // Arrange

        const string testContext1 = CONTEXT_1;
        const string testContext2 = CONTEXT_2;

        _view1.Context = testContext1;
        _view2.Context = testContext2;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);


        // Assert
        Assert.That(_view1.ViewModel.Value, Is.Not.EqualTo(_view2.ViewModel.Value));

        _coordinator.AssertContextExists(testContext1);
        _coordinator.AssertContextExists(testContext2);
        _coordinator.AssertStateConsistent();

        _coordinator.AssertViewsSharingContext(testContext1, _view1);
        _coordinator.AssertViewsSharingContext(testContext2, _view2);

        _container.Received(2).Resolve<ITestViewModel>();
    }

    #endregion

    #region Context Management

    [Test]
    public void ContextSwitching_FromWithoutContextToContext_MovesViewModelCorrectly()
    {
        // Arrange

        const string testContext = DEFAULT_CONTEXT;

        _view1.Context = testContext;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.RebindToContext(_view2, null, testContext, _container);

        // Assert
        _coordinator.AssertContextExists(testContext);

        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Has.Count.EqualTo(1));
        Assert.That(active.Single(), Is.SameAs(_viewModel1));

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));
        Assert.That(orphan.Single(), Is.SameAs(_viewModel2));

        _coordinator.AssertViewsSharingContext(testContext, _view1, _view2);

        _container.Received(2).Resolve<ITestViewModel>();
    }

    [Test]
    public void ContextSwitching_FromContextToContext_OrphansOldViewModelAndReusesNew()
    {
        // Arrange

        const string testContext1 = CONTEXT_1;
        const string testContext2 = CONTEXT_2;

        _view1.Context = testContext1;
        _view2.Context = testContext2;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.RebindToContext(_view2, _view2.Context, testContext1, _container);

        // Assert
        _coordinator.AssertContextEmpty(testContext2);
        Assert.That(_coordinator.GetAllContexts().ToHashSet(), Does.Not.Contain(testContext2));
        _coordinator.AssertContextExists(testContext1);

        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Has.Count.EqualTo(1));
        Assert.That(active.Single(), Is.SameAs(_viewModel1));

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));
        Assert.That(orphan.Single(), Is.SameAs(_viewModel2));

        _coordinator.AssertViewsSharingContext(testContext1, _view1, _view2);

        _container.Received(2).Resolve<ITestViewModel>();
    }

    [Test]
    public void ContextSwitching_ToSameContext_ReturnsCurrentViewModel()
    {
        // Arrange

        const string testContext = DEFAULT_CONTEXT;

        _view1.Context = testContext;
        _view1.InitView(_coordinator, _container);

        // Act
        _coordinator.RebindToContext(_view1, _view1.Context, testContext, _container);

        // Assert
        _coordinator.AssertContextExists(testContext);

        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Has.Count.EqualTo(1));
        Assert.That(active.Single(), Is.SameAs(_viewModel1));

        _coordinator.AssertViewsSharingContext(testContext, _view1);

        _container.Received(1).Resolve<ITestViewModel>();
    }

    [Test]
    public void GetViewModelsWithContext_WithMatcher_ReturnsFilteredResults()
    {
        // Arrange

        _view1.Context = CONTEXT_1;
        _view2.Context = CONTEXT_2;

        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        var viewModels = _coordinator.GetViewModelsWithContext(context => context == CONTEXT_1);
        var viewModels2 = _coordinator.GetViewModelsWithContext(context => context.StartsWith("test"));

        // Assert
        Assert.That(viewModels, Has.Count.EqualTo(1));
        Assert.That(viewModels.Single(), Is.EqualTo(_viewModel1));

        Assert.That(viewModels2, Has.Count.EqualTo(2));
        Assert.That(viewModels2.Select(vm => vm), Is.EquivalentTo(new[] { _viewModel1, _viewModel2 }));

        _coordinator.AssertStateConsistent();

        _container.Received(2).Resolve<ITestViewModel>();
    }

    #endregion

    #region SharedContext

    [Test]
    public void SharedContext_LastViewUnregistered_OrphansViewModel()
    {
        // Arrange

        const string testContext = DEFAULT_CONTEXT;

        _view1.Context = testContext;
        _view1.InitView(_coordinator, _container);

        // Act
        _coordinator.UnregisterView(_view1, _view1.Context);

        // Assert
        var allContexts = _coordinator.GetAllContexts().ToHashSet();
        Assert.That(allContexts, Is.Empty);

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));
        Assert.That(orphan.Single(), Is.SameAs(_viewModel1));

        _container.Received(1).Resolve<ITestViewModel>();
    }

    [Test]
    public void SharedContext_NotLastViewUnregistered_DoesNotOrphanViewModel()
    {
        // Arrange

        const string testContext = DEFAULT_CONTEXT;

        _view1.Context = testContext;
        _view2.Context = testContext;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.UnregisterView(_view2, _view2.Context);

        // Assert
        var allContexts = _coordinator.GetAllContexts().ToHashSet();
        Assert.That(allContexts, Contains.Item(testContext));

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Is.Empty);

        _container.Received(1).Resolve<ITestViewModel>();
    }

    #endregion

    #region View Unregistration

    [Test]
    public void UnregisterView_WithoutContext_OrphansViewModel()
    {
        // Arrange

        _view1.InitView(_coordinator, _container);

        // Act
        _coordinator.UnregisterView(_view1, null);

        // Assert
        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Is.Empty);

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));
        Assert.That(orphan.Single(), Is.SameAs(_viewModel1));

        _container.Received(1).Resolve<ITestViewModel>();
    }

    #endregion

    #region CoordinateRebind

    [Test]
    public void CoordinateRebind_ToViewModelWithContext_ReusesExistingViewModel()
    {
        // Arrange

        const string existingContext = EXISTING_CONTEXT;

        _view1.Context = existingContext;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.CoordinateRebind(_view2, _view1.ViewModel.Value, _container);
        _view1.ViewModel.Value.IncrementCommand.Execute(Unit.Default);

        // Assert
        Assert.That(_view1.ViewModel.Value, Is.SameAs(_view2.ViewModel.Value), "Should return the same ViewModel");
        Assert.That(_view1.Context, Is.EqualTo(existingContext), "Should have the same context");

        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Has.Count.EqualTo(1));

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));

        _container.Received(2).Resolve<ITestViewModel>();
    }

    [Test]
    public void CoordinateRebind_ToWithoutContextViewModel_CreatesTransientContext()
    {
        // Arrange

        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.CoordinateRebind(_view2, _view1.ViewModel.Value, _container);

        // Assert
        var allContexts = _coordinator.GetAllContexts().ToHashSet();
        Assert.That(allContexts.Single(), Does.StartWith("__TransientContext__"));

        var active = _coordinator.GetActive().ToHashSet();
        Assert.That(active, Has.Count.EqualTo(1));
        Assert.That(active.Single(), Is.SameAs(_viewModel1));

        var orphan = _coordinator.GetOrphan().ToHashSet();
        Assert.That(orphan, Has.Count.EqualTo(1));
        Assert.That(orphan.Single(), Is.SameAs(_viewModel2));

        _container.Received(2).Resolve<ITestViewModel>();
    }

    #endregion

    #region Error Handling

    [Test]
    public void ContainerResolve_ThrowsException_DoesNotCorruptState()
    {
        // Arrange

        _container.Resolve<ITestViewModel>().Returns(x => throw new InvalidOperationException("Container failed"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _view1.InitView(_coordinator, _container));

        _coordinator.AssertEmptyState();
        _coordinator.AssertStateConsistent();
    }

    [Test]
    public void ViewAlreadyInContext_ThrowsInvalidOperationException()
    {
        // Arrange

        _view1.Context = DEFAULT_CONTEXT;
        _view1.InitView(_coordinator, _container);

        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _coordinator.Get(_view1, "different_context", _container);
        });
    }

    #endregion

    #region Complex

    [Test]
    public void MultipleContextSwitches_FollowedByUnregistration_MaintainsConsistentState()
    {
        // Arrange

        const string testContext1 = CONTEXT_1;
        const string testContext2 = CONTEXT_2;

        _view1.Context = testContext1;
        _view2.Context = testContext2;
        _view1.InitView(_coordinator, _container);
        _view2.InitView(_coordinator, _container);

        // Act
        _coordinator.RebindToContext(_view2, _view2.Context, _view1.Context, _container);

        _coordinator.UnregisterView(_view1, _view1.Context);
        _coordinator.UnregisterView(_view2, _view2.Context);

        // Assert
        _coordinator.AssertStateConsistent();
        Assert.That(_coordinator.GetActive(), Is.Empty);
        Assert.That(_coordinator.GetOrphan().Count(), Is.EqualTo(2));
    }

    #endregion

    #region Concurrency

    [Test]
    public void ConcurrentOperations_MultipleThreads_MaintainsThreadSafety()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 20;

        // Setup container to return many ViewModels
        var viewModels = Enumerable.Range(0, threadCount * operationsPerThread)
            .Select(_ => new TestViewModel())
            .Cast<ITestViewModel>()
            .ToArray();
        _container.Resolve<ITestViewModel>().Returns(viewModels[0], viewModels.Skip(1).ToArray());

        var exceptions = new ConcurrentBag<Exception>();
        var completedOperations = new ConcurrentBag<string>();
        var random = new System.Random();

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    string operationId = $"T{threadId}_Op{i}";

                    var view = Substitute.For<IView<ITestViewModel>>();
                    view.ViewModel.Returns(new ReactiveProperty<ITestViewModel>(default));

                    try
                    {
                        int operation = random.Next(0, 4);

                        switch (operation)
                        {
                            case 0: // Create without context
                                view.InitView(_coordinator, _container);
                                completedOperations.Add($"{operationId}_CreateNoContext");
                                break;

                            case 1: // Create with context
                                view.Context = $"context_{threadId}_{i}";
                                view.InitView(_coordinator, _container);
                                completedOperations.Add($"{operationId}_CreateWithContext");
                                break;

                            case 2: // Query operations (read-only)
                                _coordinator.GetActive();
                                _coordinator.GetAllContexts();
                                _coordinator.GetViewsWithContext();
                                completedOperations.Add($"{operationId}_Query");
                                break;

                            case 3: // Create with shared context (potential contention)
                                view.Context = $"shared_context_{threadId % 3}"; // Force some sharing
                                view.InitView(_coordinator, _container);
                                completedOperations.Add($"{operationId}_SharedContext");
                                break;
                        }

                        Thread.Sleep(random.Next(0, 2));
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new Exception($"Operation {operationId} failed", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception($"Thread {threadId} failed", ex));
            }
        })).ToArray();

        Task.WaitAll(tasks, TimeSpan.FromSeconds(30));

        // Assert
        Assert.That(exceptions, Is.Empty,
            $"Concurrent operations should not throw exceptions. Exceptions: {string.Join(", ", exceptions.Select(e => e.Message))}");

        Assert.That(completedOperations.Count, Is.GreaterThan(0),
            "Some operations should have completed successfully");

        _coordinator.AssertStateConsistent();

        var totalViewModels = _coordinator.GetActive().Count() + _coordinator.GetOrphan().Count();
        Assert.That(totalViewModels, Is.GreaterThan(0), "Should have created some ViewModels");

        var allActive = _coordinator.GetActive().ToList();
        var allOrphan = _coordinator.GetOrphan().ToList();

        Assert.That(allActive.Count, Is.EqualTo(allActive.Distinct().Count()),
            "Active ViewModels should not contain duplicates");
        Assert.That(allOrphan.Count, Is.EqualTo(allOrphan.Distinct().Count()),
            "Orphaned ViewModels should not contain duplicates");
        Assert.That(allActive.Intersect(allOrphan).Count(), Is.EqualTo(0),
            "ViewModels should not be both active and orphaned");

        Debug.Log($"Completed {completedOperations.Count} concurrent operations across {threadCount} threads");
        Debug.Log($"Final state: {allActive.Count} active, {allOrphan.Count} orphaned ViewModels");
    }

    #endregion
}
