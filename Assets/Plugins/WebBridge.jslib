mergeInto(LibraryManager.library, {

  EnviarOrbesAWeb: function (cantidad) {
    // Buscamos una función en el Javascript de nuestra web Flask y le pasamos los orbes
    if (typeof window.recibirOrbesDesdeUnity === "function") {
        window.recibirOrbesDesdeUnity(cantidad);
    } else {
        console.error("Error: La web no tiene definida la función 'recibirOrbesDesdeUnity'");
    }
  },

});