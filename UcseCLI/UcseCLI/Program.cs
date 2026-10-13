using UcseCLI.Cli;

// Punto de entrada: se arma el árbol de comandos y se ejecuta el que pidió el usuario.
return Comandos.CrearRaiz().Parse(args).Invoke();
