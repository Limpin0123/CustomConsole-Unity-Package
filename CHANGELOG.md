## [1.1.5] - 2025-08-08
### Better Path Extractor Release
- The Custom Console Command Caller (CCCC) no longer create an error if the command's length is equal to 0.
- prompts that doesn't start with "/" are no longer considered as command and instead creates a Debug Log following this layout:  Say : the_prompt
- Clicking on a CCErrorLog from the CConsole now open the correct file (doesn't open the CustomLogger file anymore).
- The CConsole's Ui was made more responsive the size changes.