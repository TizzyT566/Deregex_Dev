# Deregex_Dev

A small alternative to regex.

Aimed at being a little more friendly for those of us who cant deal with regex syntax.

## Features

1. New StringRange type which is just a combination of string and range as a means to pass around ranges while still keeping a reference of the string its associated with. It is implicitly converted to type string and to type Range.

2. Has the following built in patterns:
   - None: A pattern with no definition.
   - End: Specifies the end of run.
   - Any: A pattern where the run in the string is anything until the next pattern matches.
   - Except: A pattern where the run in the string is not any of the provided parameters.
   - Text: A pattern where the run in the string has to be any of the provided parameters.
   - Repeat: A pattern where the run repeats a specified at least and/or at most times.
   