using ALX.Runtime.Values;

namespace ALX.Runtime.Values;

/// <summary>
/// Manages variable storage and scope for the ALX runtime.
/// </summary>
public class AlxEnvironment
{
    private readonly Dictionary<string, AlxValue> _variables = new();
    private readonly HashSet<string> _constants = new();
    private readonly AlxEnvironment? _parent;

    public AlxEnvironment(AlxEnvironment? parent = null)
    {
        _parent = parent;
    }

    /// <summary>
    /// Define a new variable in the current scope.
    /// </summary>
    public void Define(string name, AlxValue value, bool isConstant = false)
    {
        _variables[name] = value;
        if (isConstant)
        {
            _constants.Add(name);
        }
    }

    /// <summary>
    /// Get the value of a variable, searching up the scope chain.
    /// </summary>
    public AlxValue? Get(string name)
    {
        if (_variables.TryGetValue(name, out var value))
        {
            return value;
        }

        return _parent?.Get(name);
    }

    /// <summary>
    /// Set the value of an existing variable, searching up the scope chain.
    /// </summary>
    public bool Set(string name, AlxValue value)
    {
        if (_variables.ContainsKey(name))
        {
            if (_constants.Contains(name))
            {
                return false; // Cannot reassign constants
            }
            _variables[name] = value;
            return true;
        }

        return _parent?.Set(name, value) ?? false;
    }

    /// <summary>
    /// Check if a variable exists in the current scope.
    /// </summary>
    public bool Has(string name)
    {
        return _variables.ContainsKey(name) || (_parent?.Has(name) ?? false);
    }

    /// <summary>
    /// Get all variable names in the current scope (not parent scopes).
    /// </summary>
    public IEnumerable<string> GetLocalNames()
    {
        return _variables.Keys;
    }

    /// <summary>
    /// Get all variables in the current scope.
    /// </summary>
    public Dictionary<string, AlxValue> GetAll()
    {
        return new Dictionary<string, AlxValue>(_variables);
    }

    /// <summary>
    /// Parent environment (for scope chain traversal).
    /// </summary>
    public AlxEnvironment? Parent => _parent;
}
