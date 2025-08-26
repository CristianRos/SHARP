using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using R3;
using SHARP.Core;
using SHARP.Tests.Utils;
using UnityEngine;

public class DiscoveryTests
{
    #region Fields

    // Hierarchy
    Transform _root;
    Transform _childA, _childB, _childC;
    Transform _grandChildC1, _grandChildC2;

    // SHARP
    IContainer _container;
    Coordinator<ITestViewModel> _coordinator;

    #endregion

    #region Setup and TearDown

    [SetUp]
    public void Setup()
    {
        SetupCoordinatorAndDiscovery();
        SetupTestHierarchy();
        SetupViews();
    }

    void SetupCoordinatorAndDiscovery()
    {
        _container = Substitute.For<IContainer>();
        _coordinator = new();

        _container
            .Resolve<ITestViewModel>()
            .Returns(_ => new TestViewModel());
    }

    void SetupTestHierarchy()
    {
        //         _root
        //        ├─ _childA (viewA, Context_X)  <- unregistered
        //        ├─ _childB (viewB)
        //        │   └─ _childC (viewC, Context_Y)
        //        │       ├─ _grandChildC1 (viewC1)
        //        │       └─ _grandChildC2 (viewC2)

        _root = new GameObject("Root").transform;
        _childA = new GameObject("ChildA").transform;
        _childB = new GameObject("ChildB").transform;
        _childC = new GameObject("ChildC").transform;
        _grandChildC1 = new GameObject("GrandChildC1").transform;
        _grandChildC2 = new GameObject("GrandChildC2").transform;

        _root.SetParent(null);
        _childA.SetParent(_root);
        _childB.SetParent(_root);
        _childC.SetParent(_childB);
        _grandChildC1.SetParent(_childC);
        _grandChildC2.SetParent(_childC);
    }

    void SetupViews()
    {
        var viewA = _childA.gameObject.AddComponent<TestView>();
        var viewB = _childB.gameObject.AddComponent<TestView>();
        var viewC = _childC.gameObject.AddComponent<TestView>();
        var viewC1 = _grandChildC1.gameObject.AddComponent<TestView>();
        var viewC2 = _grandChildC2.gameObject.AddComponent<TestView>();

        viewA.InitView(_coordinator, _container, "Context_X");
        viewB.InitView(_coordinator, _container);
        viewC.InitView(_coordinator, _container, "Context_Y");
        viewC1.InitView(_coordinator, _container);
        viewC2.InitView(_coordinator, _container);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _container?.Dispose();
            _coordinator?.Dispose();

            _coordinator?.AssertEmptyState();

            if (_root != null)
            {
                GameObject.DestroyImmediate(_root.gameObject);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception thrown during teardown: {ex}");
        }
    }

    #endregion

    #region Combined Constraints

    [Test]
    public void SpatialAndState_QueryAfterOrphaning_ReturnsDescendantsOfActiveViewModels()
    {
        // Arrange
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);

        var viewA = _childA.GetComponent<TestView>();
        _coordinator.UnregisterView(viewA, viewA.Context);

        // Act
        var results = discovery
            .DescendantsOf(_root, 3)
            .ThatAreActive()
            .All();

        // Assert
        Assert.That(results, Has.Count.EqualTo(4));
    }

    [Test]
    public void AllConstraints_QueryAfterComplexChanges_ReturnsMatchingViewModels()
    {
        // Arrange
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);

        var viewA = _childA.GetComponent<TestView>();
        var viewB = _childB.GetComponent<TestView>();
        var viewC = _childC.GetComponent<TestView>();
        var viewC1 = _grandChildC1.GetComponent<TestView>();
        var viewC2 = _grandChildC2.GetComponent<TestView>();


        _coordinator.RebindToContext(viewC1, viewC1.Context, "Context_X", _container);

        viewC1.ViewModel.Value.IncrementCommand.Execute(Unit.Default);
        viewC2.ViewModel.Value.IncrementCommand.Execute(Unit.Default);

        // Act
        var results = discovery
            .ChildrenOf(_childC)
            .WithoutContext()
            .Where(vm => vm.TestProperty.Value > 0)
            .All();

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results.Single(), Is.EqualTo(viewC2.ViewModel.Value));
        Assert.That(results.Single().TestProperty.Value, Is.EqualTo(1));
    }

    [Test]
    public void ComplexQuery_AfterSpatialAndContextChanges_ReturnsSuccessfulQuery()
    {
        // Arrange
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);

        var viewA = _childA.GetComponent<TestView>();
        var viewB = _childB.GetComponent<TestView>();
        var viewC = _childC.GetComponent<TestView>();
        var viewC1 = _grandChildC1.GetComponent<TestView>();
        var viewC2 = _grandChildC2.GetComponent<TestView>();

        _coordinator.RebindToContext(viewB, viewB.Context, "NewContext_A", _container);
        _coordinator.RebindToContext(viewC1, viewC1.Context, "NewContext_A", _container);
        _coordinator.RebindToContext(viewC2, viewC2.Context, "NewContext_B", _container);

        _childC.SetParent(_childA);
        _grandChildC1.SetParent(_childB);


        viewC1.ViewModel.Value.TestProperty.Value = 5;

        // This hierarchy should now be:
        // _root
        //  _childA (Context_X)
        //      _childC (Context_Y)
        //          _grandChildC2 (NewContext_B)
        //  _childB (NewContext_A)
        //      _grandChildC1 (NewContext_A)

        // Act
        var results = discovery
            .DescendantsOf(_root)
            .WhereContext(context => context.Contains("_X") || context.Contains("_Y"))
            .All();

        var results2 = discovery
            .ChildrenOf(_childC)
            .WhereContext(ctx => ctx.Contains("_B"))
            .FirstOrDefault();

        var results3 = discovery
            .ChildrenOf(_childB)
            .WhereContext(ctx => ctx.Contains("_A"))
            .Any();

        var results4 = discovery
            .ChildrenOf(_childB)
            .WhereContext(ctx => ctx.Contains("_A"))
            .Where(vm => vm.TestProperty.Value > 4)
            .Single();

        // Assert
        Assert.That(results, Has.Count.EqualTo(2), "Should find 2 ViewModels in NewContext_B and NewContext_A");
        Assert.That(results2, Is.Not.Null, "Should find a ViewModel in NewContext_B");
        Assert.That(results3, Is.Not.Null, "Should find a ViewModel in NewContext_A");
        Assert.That(results4, Is.Not.Null, "Should find a ViewModel in NewContext_A with TestProperty > 4");
    }

    #endregion

    #region Performance Tests

    [Test]
    public void Performance_SmallHierarchy_BaselinePerformance()
    {
        // Baseline: Current test setup (5 views, 4 contexts)
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Run multiple iterations to get average
        const int iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            var results = discovery
                .DescendantsOf(_root)
                .WhereContext(ctx => ctx.Contains("Context"))
                .Where(vm => vm.TestProperty.Value >= 0)
                .All();
        }

        stopwatch.Stop();
        var avgMs = stopwatch.ElapsedMilliseconds / (double)iterations;

        Debug.Log($"Baseline Performance: {avgMs:F3}ms per query (5 views, 4 contexts)");
        Assert.That(avgMs, Is.LessThan(1.0), "Baseline queries should be sub-millisecond");
    }

    [Test]
    public void Performance_MediumHierarchy_ScalingTest()
    {
        // Setup: 50 views in a deeper hierarchy
        var mediumRoot = CreateMediumTestHierarchy();
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 100;

        for (int i = 0; i < iterations; i++)
        {
            var results = discovery
                .DescendantsOf(mediumRoot)
                .WhereContext(ctx => ctx.StartsWith("Level"))
                .Where(vm => vm.TestProperty.Value < 25)
                .All();
        }

        stopwatch.Stop();
        var avgMs = stopwatch.ElapsedMilliseconds / (double)iterations;

        Debug.Log($"Medium Scale Performance: {avgMs:F3}ms per query (50 views)");
        Assert.That(avgMs, Is.LessThan(10.0), "Medium queries should be under 10ms");

        // Cleanup
        GameObject.DestroyImmediate(mediumRoot.gameObject);
    }

    [Test]
    public void Performance_LargeHierarchy_StressTest()
    {
        // Setup: 200 views across multiple levels
        var largeRoot = CreateLargeTestHierarchy();
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 10; // Fewer iterations for stress test

        for (int i = 0; i < iterations; i++)
        {
            var results = discovery
                .DescendantsOf(largeRoot)
                .WhereContext(ctx => ctx.Contains("Branch"))
                .Where(vm => vm.TestProperty.Value % 10 == 0)
                .All();
        }

        stopwatch.Stop();
        var avgMs = stopwatch.ElapsedMilliseconds / (double)iterations;

        Debug.Log($"Large Scale Performance: {avgMs:F3}ms per query (200 views)");
        Assert.That(avgMs, Is.LessThan(50.0), "Large queries should complete within 50ms");

        // Cleanup
        GameObject.DestroyImmediate(largeRoot.gameObject);
    }

    [Test]
    public void Performance_QueryTypeComparison_IdentifyBottlenecks()
    {
        var mediumRoot = CreateMediumTestHierarchy();
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);
        const int iterations = 100;

        // Test 1: Spatial only
        var spatialWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            discovery.DescendantsOf(mediumRoot).All();
        }
        spatialWatch.Stop();
        var spatialAvg = spatialWatch.ElapsedMilliseconds / (double)iterations;

        // Test 2: Context only
        var contextWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            discovery.WhereContext(ctx => ctx.StartsWith("Level")).All();
        }
        contextWatch.Stop();
        var contextAvg = contextWatch.ElapsedMilliseconds / (double)iterations;

        // Test 3: Predicate only
        var predicateWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            discovery.Where(vm => vm.TestProperty.Value < 25).All();
        }
        predicateWatch.Stop();
        var predicateAvg = predicateWatch.ElapsedMilliseconds / (double)iterations;

        // Test 4: Combined (worst case)
        var combinedWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            discovery
                .DescendantsOf(mediumRoot)
                .WhereContext(ctx => ctx.StartsWith("Level"))
                .Where(vm => vm.TestProperty.Value < 25)
                .All();
        }
        combinedWatch.Stop();
        var combinedAvg = combinedWatch.ElapsedMilliseconds / (double)iterations;

        Debug.Log($"Performance Breakdown (50 views):");
        Debug.Log($"  Spatial only: {spatialAvg:F3}ms");
        Debug.Log($"  Context only: {contextAvg:F3}ms");
        Debug.Log($"  Predicate only: {predicateAvg:F3}ms");
        Debug.Log($"  Combined: {combinedAvg:F3}ms");

        // Identify the bottleneck
        var slowest = Math.Max(Math.Max(spatialAvg, contextAvg), Math.Max(predicateAvg, combinedAvg));
        if (slowest == spatialAvg)
            Debug.Log("BOTTLENECK: Spatial queries (hierarchy traversal)");
        else if (slowest == contextAvg)
            Debug.Log("BOTTLENECK: Context queries (coordinator lookups)");
        else if (slowest == predicateAvg)
            Debug.Log("BOTTLENECK: Predicate filtering (VM property access)");
        else
            Debug.Log("BOTTLENECK: Combined query overhead");

        // Sanity checks
        Assert.That(spatialAvg, Is.LessThan(5.0), "Spatial queries shouldn't be the bottleneck");
        Assert.That(contextAvg, Is.LessThan(2.0), "Context queries should be fastest");

        GameObject.DestroyImmediate(mediumRoot.gameObject);
    }

    [Test]
    public void Performance_QueryMethodComparison_OptimalExecution()
    {
        var mediumRoot = CreateMediumTestHierarchy();
        var discovery = new DiscoveryQuery<ITestViewModel>(_coordinator);
        const int iterations = 100;

        var query = discovery
            .DescendantsOf(mediumRoot)
            .WhereContext(ctx => ctx.StartsWith("Level"));

        // Test All()
        var allWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            query.All();
        }
        allWatch.Stop();

        // Test Count()
        var countWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            query.Count();
        }
        countWatch.Stop();

        // Test Any()
        var anyWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            query.Any();
        }
        anyWatch.Stop();

        // Test FirstOrDefault()
        var firstWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            query.FirstOrDefault();
        }
        firstWatch.Stop();

        var allAvg = allWatch.ElapsedMilliseconds / (double)iterations;
        var countAvg = countWatch.ElapsedMilliseconds / (double)iterations;
        var anyAvg = anyWatch.ElapsedMilliseconds / (double)iterations;
        var firstAvg = firstWatch.ElapsedMilliseconds / (double)iterations;

        Debug.Log($"Query Method Performance:");
        Debug.Log($"  All(): {allAvg:F3}ms");
        Debug.Log($"  Count(): {countAvg:F3}ms");
        Debug.Log($"  Any(): {anyAvg:F3}ms");
        Debug.Log($"  FirstOrDefault(): {firstAvg:F3}ms");

        // Any() and FirstOrDefault() should be faster (early termination)
        Assert.That(anyAvg, Is.LessThanOrEqualTo(allAvg), "Any() should be faster than All()");
        Assert.That(firstAvg, Is.LessThanOrEqualTo(allAvg), "FirstOrDefault() should be faster than All()");

        GameObject.DestroyImmediate(mediumRoot.gameObject);
    }

    #region Helper Methods for Performance Tests

    Transform CreateMediumTestHierarchy()
    {
        // Creates: 1 root + 5 branches + 10 leaves each = 51 total GameObjects
        var root = new GameObject("PerfTestRoot_Medium").transform;

        for (int branch = 0; branch < 5; branch++)
        {
            var branchObj = new GameObject($"Branch_{branch}").transform;
            branchObj.SetParent(root);

            var branchView = branchObj.gameObject.AddComponent<TestView>();
            branchView.InitView(_coordinator, _container, $"Level_{branch}_Branch");

            for (int leaf = 0; leaf < 10; leaf++)
            {
                var leafObj = new GameObject($"Leaf_{branch}_{leaf}").transform;
                leafObj.SetParent(branchObj);

                var leafView = leafObj.gameObject.AddComponent<TestView>();

                // Mix of contexts and no contexts
                if (leaf % 3 == 0)
                    leafView.InitView(_coordinator, _container, $"Level_{branch}_Item_{leaf}");
                else
                    leafView.InitView(_coordinator, _container);

                // Set some test data
                leafView.ViewModel.Value.TestProperty.Value = branch * 10 + leaf;
            }
        }

        return root;
    }

    Transform CreateLargeTestHierarchy()
    {
        // Creates: 1 root + 10 branches + 20 leaves each = 201 total GameObjects  
        var root = new GameObject("PerfTestRoot_Large").transform;

        for (int branch = 0; branch < 10; branch++)
        {
            var branchObj = new GameObject($"Branch_{branch}").transform;
            branchObj.SetParent(root);

            var branchView = branchObj.gameObject.AddComponent<TestView>();
            branchView.InitView(_coordinator, _container, $"Branch_{branch}_Main");

            for (int leaf = 0; leaf < 20; leaf++)
            {
                var leafObj = new GameObject($"Item_{branch}_{leaf}").transform;
                leafObj.SetParent(branchObj);

                var leafView = leafObj.gameObject.AddComponent<TestView>();

                // Various context patterns
                if (leaf % 4 == 0)
                    leafView.InitView(_coordinator, _container, $"Branch_{branch}_Special_{leaf}");
                else if (leaf % 2 == 0)
                    leafView.InitView(_coordinator, _container, $"Branch_{branch}_Normal");
                else
                    leafView.InitView(_coordinator, _container);

                leafView.ViewModel.Value.TestProperty.Value = branch * 100 + leaf;
            }
        }

        return root;
    }

    #endregion

    #endregion
}