## What

A simple command line application in .NET Core.
The goal is to calculate a currency exchange using a file.

The file must be formatted like so:
  - StartCurrency;Amount;TargetCurrency
  - NumberOfExchangeRate
  - CurrencyA;CurrencyB;ExchangeRate
  - CurrencyX;CurrencyY;ExchangeRate
  - etc

Example
```
EUR;550;JPY
6
AUD;CHF;0.9661
JPY;KRW;13.1151
EUR;CHF;1.2053
AUD;JPY;86.0305
EUR;USD;1.2989
JPY;INR;0.6571
```

## Usage

1. Download zip
2. Navigate to "bin/Release/net5.0" via terminal
3. Launch either "./LuccaDevise.exe \<pathToFile\>" or "dotnet ./LuccaDevises.dll \<pathToFile\>" command

## Tests

Some unit tests are available.
