# SHARP

**SHARP** is a reactive MVVM programming library for Unity, leveraging the power of Cysharp's [R3](https://github.com/Cysharp/R3) for high-performance reactivity, the Reflex DI framework for dependency injection, and the MVVM paradigm to build robust, reactive, and maintainable UI architectures.

---

## Key Components

### MVVM Base Classes

- **Model.cs**  
  The base class for application data logic. Encapsulates state and core business logic for your MVVM architecture.

- **ViewModel.cs**  
  The reactive mediator between your Views and Models. Utilizes R3 to expose observable properties, commands, and state, making UI updates automatic and declarative.

- **View.cs**  
  The Unity MonoBehaviour base class for all Views. Handles data binding, subscribes to ViewModel changes, and manages the visual presentation. Designed for easy extension and flexible binding.

### Coordinator Service

- **Coordinator**  
  The Coordinator service manages the lifecycle and relationships between ViewModels and Views.  
  - Tracks creation, destruction, and active states of MVVM components.
  - Enables context switching, and ensures consistency across the UI.
  - Acts as the central hub for MVVM orchestration in your Unity scene.

### Discovery Service

- **Discovery**  
  The Discovery service provides a fluent query builder API on top of the Coordinator and Unity's scene hierarchy.
  - Enables powerful search and retrieval of ViewModels, Views, and their relationships.
  - Facilitates dynamic UI composition, context-aware queries, and advanced MVVM scenarios.

### Helpers & Integrations

- **Helpers**  
  Utility functions and extensions to simplify common MVVM, reactivity, and Unity integration tasks.

- **Integrations**  
  Bridges between SHARP and external frameworks or Unity systems (e.g., Reflex DI integration).

---

## Features

- **Reactive Data Binding:**  
  Built-in support for observable properties and commands via Cysharp R3.

- **MVVM Out-of-the-Box:**  
  Complete suite of Model, ViewModel, and View base classes for rapid MVVM development.

- **Lifecycle & Relation Tracking:**  
  Coordinator service transparently manages View/ViewModel mapping and lifetimes.

- **Fluent Discovery API:**  
  Easily search, filter, and manipulate ViewModels and Views using expressive queries.

- **Unity-First Design:**  
  Seamless integration with the Unity Editor, scene hierarchy, and MonoBehaviours.

---

## Getting Started

1. **Install SHARP**  
   Add the SHARP package to your Unity project.

2. **Setup Models, ViewModels, and Views**  
   - Derive your logic from `Model.cs`, `ViewModel.cs`, and `View.cs`.
   - Use R3 observables for any property/state you want to react to.

3. **Use the Coordinator and Discovery Services**  
   - Register your Views and ViewModels.
   - Query and compose UI dynamically at runtime.

4. **Integrate Reflex DI**  
   - Leverage Reflex for dependency injection in your MVVM components.

---

## Example

### Basic Counter Example

```csharp
// VM_Counter.cs
using R3;
using SHARP.Core;

public class VM_Counter : ViewModel
{
    ReactiveProperty<int> _count = new(0);
    public ReactiveProperty<string> DisplayCount = new($"Count: 0");

    public ReactiveCommand Increase { get; private set; } = new();
    public ReactiveCommand Decrease { get; private set; } = new();

    protected override void HandleSubscriptions(ref DisposableBuilder d)
    {
        _count
            .Subscribe(value => DisplayCount.Value = $"Count: {value}")
            .AddTo(ref d);

        Increase
            .Subscribe(_ => _count.Value++)
            .AddTo(ref d);

        Decrease
            .Subscribe(_ => _count.Value--)
            .AddTo(ref d);
    }
}

// V_Counter.cs
using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class V_Counter : View<VM_Counter>
{
    [SerializeField] TMP_Text _countText;
    [SerializeField] Button _increaseButton;
    [SerializeField] Button _decreaseButton;

    protected override void HandleSubscriptions(VM_Counter viewModel, ref DisposableBuilder d)
    {
        viewModel.DisplayCount
            .Subscribe(value => _countText.text = value)
            .AddTo(ref d);

        _increaseButton.OnClickAsObservable()
            .Subscribe(viewModel.Increase.Execute)
            .AddTo(ref d);

        _decreaseButton.OnClickAsObservable()
            .Subscribe(viewModel.Decrease.Execute)
            .AddTo(ref d);
    }
}
```

---

For a full suite of advanced and practical examples—including scenarios like counters with context, slider-based counters, dynamic rebinding, and discovery—explore the [`Assets/SHARP/Examples`](https://github.com/CristianRos/SHARP/tree/main/Assets/SHARP/Examples) folder.  
**Note:** Only a subset of results can be shown here. [View more example files in GitHub's code search.](https://github.com/search?q=repo%3ACristianRos%2FSHARP+path%3A%2FAssets%2FSHARP%2FExamples%2F&type=code)

---

## Core Directory Structure

- `Coordinator/` — Coordinator service code
- `Discovery/` — Discovery service code
- `Helpers/` — Utility helpers
- `Integrations/` — Integration points for external systems
- `Model.cs` — Base Model class
- `ViewModel.cs` — Base ViewModel class
- `View.cs` — Base View class

---

## Credits

- [Cysharp R3](https://github.com/Cysharp/R3) — High-performance reactive library for .NET/Unity.
- [Reflex DI](https://github.com/zaafar/Reflex) — Lightweight dependency injection for Unity.

---

## License

MIT

