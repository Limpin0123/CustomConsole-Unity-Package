# Custom Console

A customizable in-game console for Unity that lets you call methods at runtime via typed commands — ideal for debugging builds or exposing developer tools.

---

## ✨ Features

- Execute public methods via a simple in-game console
- Autocomplete suggestions while typing commands
- Logs and error messages displayed inside the console
- Supports common parameter types: `int`, `float`, `string`, etc.
- Organized by script and method name for clarity

---

## 🚀 Installation

To install via Unity Package Manager:

1. Open your project’s `manifest.json` file (in `Packages/`).
2. Add the following line inside the `"dependencies"` block:

```json
"com.limpin.customconsole": "file:../CustomConsole"
```

## 🔧 Usage

1. Add the console prefab to your scene (or instantiate it via script).
2. Use the [CallableFunction("YourCommandName")] attribute on public methods you want to expose.
3. Open the console in play mode and type /YourCommandName to call the function.

## Example
```C#
    [CallableFunction("Jump")]
    public void Jump()
    {
        player.Jump();
    }
```
You can also call methods with parameters, like:
```
    /SetHealth 50
```
## Supported Parameter Types
The console supports methods with parameters of the following types:

int

float

bool

string

Vector2

Vector3

Color

enum (basic ones)

If a method contains unsupported types, it will be ignored, and you’ll get a warning in the editor.

## 📄 License
This package is licensed under CC BY-NC 4.0.
You may use and modify it for non-commercial projects, with attribution.



