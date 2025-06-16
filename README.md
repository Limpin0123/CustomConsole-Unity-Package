# Custom Console

A customizable in-game console for Unity that lets you call methods at runtime via typed commands — ideal for debugging builds or exposing developer tools.

---

## ✨ Features

- Execute public, private and static methods via a simple in-game console
- Basic autocomplete suggestions while typing commands
- Logs and error messages displayed inside the console
- Supports common parameter types: `int`, `float`, `string`, etc.
- Organized by script and method name for clarity

---

## 🚀 Installation

To install via Unity Package Manager:

1. Go to package manager
2. Select "Import from GIT URL" and past the following URL :
```json
https://github.com/Limpin0123/CustomConsole.git
```

## 🔧 Usage

1. Add the console prefab to your scene => right click in hierarchie or instantiate it via script.
2. Use the [CallableFunction("YourCommandName")] attribute on public, private or static methods you want to expose.
3. Open the console in play mode and type /YourCommandName to call the function. For function with parameters, place them after the command's name and separate with spaces (string can be surrounded by quotation mark)

## Example
```C#
    [CallableFunction("Jump")]
    private void Jump()
    {
        player.Jump();
    }

    [CallableFunction("SetHealth")]
    public void SettingPlayerHealth(int pv)
    {
        player.healthPoint = pv;
    }
```
method will be called, like:
```
    /Jump
    /SetHealth 50
```
## Supported Parameter Types
The console supports methods with parameters of the following types:

```
int
float
bool
string
Vector2
Vector3
Color
enum (basic ones)
```

If a method contains unsupported types, it will be ignored, and you’ll get a warning in the editor.

## 📄 License
This package is licensed under CC BY-NC 4.0.
You may use and modify it for non-commercial projects, with attribution.



