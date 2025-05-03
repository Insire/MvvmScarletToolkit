```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
AMD Ryzen 7 9800X3D, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                             | Mean      | Error     | StdDev    | Ratio | RatioSD |
|----------------------------------- |----------:|----------:|----------:|------:|--------:|
| MediatRBuildServiceProvider        | 19.422 μs | 0.3824 μs | 0.5605 μs |  1.00 |    0.04 |
| SimpleMediatorBuildServiceProvider |  2.601 μs | 0.2714 μs | 0.8003 μs |  0.13 |    0.04 |
