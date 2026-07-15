using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HetznerCloud.Core;

/// <summary>
/// Represents either a successful value or an error.
/// Inspired by Rust's Result&lt;T, E&gt; and functional programming patterns.
/// Provides compile-time safety for error handling without exceptions.
/// </summary>
/// <typeparam name="T">The success value type</typeparam>
/// <typeparam name="E">The error type</typeparam>
[Serializable]
public readonly struct Result<T, E> : IEquatable<Result<T, E>>
{
    private readonly T? _value;
    private readonly E? _error;
    private readonly bool _isSuccess;

    private Result(T value)
    {
        _value = value;
        _error = default;
        _isSuccess = true;
    }

    private Result(E error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public T Value
    {
        get
        {
            if (!_isSuccess)
                throw new InvalidOperationException("Cannot access Value on a failed Result. Check IsSuccess first.");
            return _value!;
        }
    }

    public E Error
    {
        get
        {
            if (_isSuccess)
                throw new InvalidOperationException("Cannot access Error on a successful Result. Check IsFailure first.");
            return _error!;
        }
    }

    public static Result<T, E> Success(T value) => new(value);
    public static Result<T, E> Failure(E error) => new(error);

    public static implicit operator Result<T, E>(T value) => Success(value);
    public static implicit operator Result<T, E>(E error) => Failure(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<E, TResult> onFailure)
        => _isSuccess ? onSuccess(_value!) : onFailure(_error!);

    public void Match(Action<T> onSuccess, Action<E> onFailure)
    {
        if (_isSuccess) onSuccess(_value!);
        else onFailure(_error!);
    }

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<E, Task<TResult>> onFailure)
        => _isSuccess ? await onSuccess(_value!) : await onFailure(_error!);

    public async Task MatchAsync(Func<T, Task> onSuccess, Func<E, Task> onFailure)
    {
        if (_isSuccess) await onSuccess(_value!);
        else await onFailure(_error!);
    }

    public Result<TResult, E> Map<TResult>(Func<T, TResult> mapper)
        => _isSuccess ? Result<TResult, E>.Success(mapper(_value!)) : Result<TResult, E>.Failure(_error!);

    public async Task<Result<TResult, E>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
        => _isSuccess ? Result<TResult, E>.Success(await mapper(_value!)) : Result<TResult, E>.Failure(_error!);

    public Result<T, EResult> MapError<EResult>(Func<E, EResult> mapper)
        => _isSuccess ? Result<T, EResult>.Success(_value!) : Result<T, EResult>.Failure(mapper(_error!));

    public Result<TResult, E> Bind<TResult>(Func<T, Result<TResult, E>> binder)
        => _isSuccess ? binder(_value!) : Result<TResult, E>.Failure(_error!);

    public async Task<Result<TResult, E>> BindAsync<TResult>(Func<T, Task<Result<TResult, E>>> binder)
        => _isSuccess ? await binder(_value!) : Result<TResult, E>.Failure(_error!);

    public T GetValueOrDefault() => _isSuccess ? _value! : default!;
    public T GetValueOrDefault(T defaultValue) => _isSuccess ? _value! : defaultValue;

    public T GetValueOrThrow()
    {
        if (!_isSuccess)
            throw new ResultFailureException<E>(_error!);
        return _value!;
    }

    public Option<T> ToOption() => _isSuccess ? Option<T>.Some(_value!) : Option<T>.None;

    public bool Equals(Result<T, E> other)
    {
        if (_isSuccess != other._isSuccess) return false;
        if (!_isSuccess) return EqualityComparer<E>.Default.Equals(_error!, other._error!);
        return EqualityComparer<T>.Default.Equals(_value!, other._value!);
    }

    public override bool Equals(object? obj) => obj is Result<T, E> other && Equals(other);
    public override int GetHashCode() => _isSuccess 
        ? HashCode.Combine(_isSuccess, _value) 
        : HashCode.Combine(_isSuccess, _error);

    public static bool operator ==(Result<T, E> left, Result<T, E> right) => left.Equals(right);
    public static bool operator !=(Result<T, E> left, Result<T, E> right) => !left.Equals(right);

    public override string ToString() => _isSuccess 
        ? $"Success({_value})" 
        : $"Failure({_error})";
}

/// <summary>
/// Exception thrown when accessing Value on a failed Result
/// </summary>
[Serializable]
public class ResultFailureException<E> : Exception
{
    public E Error { get; }

    public ResultFailureException(E error) 
        : base($"Result contains error: {error}")
    {
        Error = error;
    }

    public ResultFailureException(E error, Exception inner) 
        : base($"Result contains error: {error}", inner)
    {
        Error = error;
    }
}

/// <summary>
/// Result without a success value (void-like)
/// </summary>
[Serializable]
public readonly struct Result<E> : IEquatable<Result<E>>
{
    private readonly E? _error;
    private readonly bool _isSuccess;

    public Result() { _isSuccess = true; }
    private Result(E error) { _error = error; _isSuccess = false; }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public E Error => _isSuccess ? throw new InvalidOperationException("No error on success") : _error!;

    public static Result<E> Success() => new();
    public static Result<E> Failure(E error) => new(error);
    public static implicit operator Result<E>(E error) => Failure(error);

    public void Match(Action onSuccess, Action<E> onFailure)
    {
        if (_isSuccess) onSuccess();
        else onFailure(_error!);
    }

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<E, TResult> onFailure)
        => _isSuccess ? onSuccess() : onFailure(_error!);

    public async Task MatchAsync(Func<Task> onSuccess, Func<E, Task> onFailure)
    {
        if (_isSuccess) await onSuccess();
        else await onFailure(_error!);
    }

    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> onSuccess, Func<E, Task<TResult>> onFailure)
        => _isSuccess ? await onSuccess() : await onFailure(_error!);

    public Result<T, E> Map<T>(Func<T> factory)
        => _isSuccess ? Result<T, E>.Success(factory()) : Result<T, E>.Failure(_error!);

    public async Task<Result<T, E>> MapAsync<T>(Func<Task<T>> factory)
        => _isSuccess ? Result<T, E>.Success(await factory()) : Result<T, E>.Failure(_error!);

    public Result<EResult> MapError<EResult>(Func<E, EResult> mapper)
        => _isSuccess ? Result<EResult>.Success() : Result<EResult>.Failure(mapper(_error!));

    public Result<E> Bind(Func<Result<E>> binder)
        => _isSuccess ? binder() : Result<E>.Failure(_error!);

    public async Task<Result<E>> BindAsync(Func<Task<Result<E>>> binder)
        => _isSuccess ? await binder() : ResultExtensions.FailureOf(_error!);

    public bool Equals(Result<E> other) => _isSuccess == other._isSuccess && 
        (_isSuccess || EqualityComparer<E>.Default.Equals(_error!, other._error!));

    public override bool Equals(object? obj) => obj is Result<E> other && Equals(other);
    public override int GetHashCode() => _isSuccess ? 1 : HashCode.Combine(_error);
    public static bool operator ==(Result<E> left, Result<E> right) => left.Equals(right);
    public static bool operator !=(Result<E> left, Result<E> right) => !left.Equals(right);

    public override string ToString() => _isSuccess ? "Success" : $"Failure({_error})";
}

/// <summary>
/// Optional value type (similar to Option&lt;T&gt; in Rust/F#)
/// </summary>
[Serializable]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Option(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public bool HasValue => _hasValue;
    public bool IsNone => !_hasValue;
    public T Value => _hasValue ? _value! : throw new InvalidOperationException("Option has no value");
    public T ValueOrDefaultValue => _value!;
    public T GetValueOrDefault(T defaultValue) => _hasValue ? _value! : defaultValue;

    public static Option<T> Some(T value) => new(value);
    public static Option<T> None => default;
    public static implicit operator Option<T>(T value) => value is null ? None : Some(value);

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
        => _hasValue ? some(_value!) : none();

    public void Match(Action<T> some, Action none)
    {
        if (_hasValue) some(_value!);
        else none();
    }

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none)
        => _hasValue ? await some(_value!) : await none();

    public async Task MatchAsync(Func<T, Task> some, Func<Task> none)
    {
        if (_hasValue) await some(_value!);
        else await none();
    }

    public Option<TResult> Map<TResult>(Func<T, TResult> mapper)
        => _hasValue ? Option<TResult>.Some(mapper(_value!)) : Option<TResult>.None;

    public async Task<Option<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
        => _hasValue ? Option<TResult>.Some(await mapper(_value!)) : Option<TResult>.None;

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
        => _hasValue ? binder(_value!) : Option<TResult>.None;

    public async Task<Option<TResult>> BindAsync<TResult>(Func<T, Task<Option<TResult>>> binder)
        => _hasValue ? await binder(_value!) : Option<TResult>.None;

    public Option<T> Filter(Func<T, bool> predicate)
        => _hasValue && predicate(_value!) ? this : Option<T>.None;

    public T GetValueOrThrow(string? paramName = null)
    {
        if (!_hasValue)
            throw new InvalidOperationException($"Option was None{(paramName is not null ? $" ({paramName})" : "")}");
        return _value!;
    }

    public Option<T> Or(Option<T> alternative) => _hasValue ? this : alternative;
    public Option<T> OrElse(Func<Option<T>> factory) => _hasValue ? this : factory();

    public Result<T, E> ToResult<E>(E error) => _hasValue 
        ? Result<T, E>.Success(_value!) 
        : Result<T, E>.Failure(error);

    public bool Equals(Option<T> other)
        => _hasValue == other._hasValue && (!_hasValue || EqualityComparer<T>.Default.Equals(_value!, other._value!));

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);
    public override int GetHashCode() => _hasValue ? HashCode.Combine(_value) : 0;
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);

    public override string ToString() => _hasValue ? $"Some({_value})" : "None";
}

/// <summary>
    /// Non-generic Option helpers
    /// </summary>
    public static class OptionExtensions
    {
        public static Option<T> Some<T>(T value) => Option<T>.Some(value);
        public static Option<T> None<T>() => Option<T>.None;
        public static Option<T> FromNullable<T>(T? value) => value is null ? Option<T>.None : Option<T>.Some(value);
    }

    /// <summary>
    /// Non-generic Result helpers
    /// </summary>
    public static class ResultExtensions
    {
        public static Result<T, E> SuccessOf<T, E>(T value) => Result<T, E>.Success(value);
        public static Result<T, E> FailureOf<T, E>(E error) => Result<T, E>.Failure(error);
        public static Result<E> SuccessOf<E>() => Result<E>.Success();
        public static Result<E> FailureOf<E>(E error) => Result<E>.Failure(error);
        public static Result<T, E> FromNullable<T, E>(T? value, E error) => value is null ? FailureOf<T, E>(error) : SuccessOf<T, E>(value);
        public static async Task<Result<T, E>> TryAsync<T, E>(Func<Task<T>> func, Func<Exception, E> errorMapper)
        {
            try { return SuccessOf<T, E>(await func()); }
            catch (Exception ex) { return FailureOf<T, E>(errorMapper(ex)); }
        }
        public static async Task<Result<E>> TryAsync<E>(Func<Task> func, Func<Exception, E> errorMapper)
        {
            try { await func(); return SuccessOf<E>(); }
            catch (Exception ex) { return FailureOf<E>(errorMapper(ex)); }
        }
    }