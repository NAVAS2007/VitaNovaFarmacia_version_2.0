// FUNCIONES GENERALES
function formatCurrency(amount) {
    return '$' + Number(amount).toFixed(2);
}

// FUNCIONES DEL POS
var posConfig = {
    searchDebounce: 300,
    selectedItemIndex: -1
};

function buscarProductos(termino) {
    clearTimeout(posConfig.searchTimeout);
    var resultadosDiv = document.getElementById('resultadosBusqueda');

    if (!termino || termino.length < 2) {
        resultadosDiv.style.display = 'none';
        posConfig.selectedItemIndex = -1;
        return;
    }

    posConfig.searchTimeout = setTimeout(function () {
        fetch('/Ventas/Carrito?handler=Buscar&termino=' + encodeURIComponent(termino))
            .then(response => response.json())
            .then(data => {
                posConfig.selectedItemIndex = -1;

                if (data.length > 0) {
                    resultadosDiv.innerHTML = data.map((p, index) =>
                        `<button class="list-group-item list-group-item-action resultado-busqueda" 
                                 data-index="${index}"
                                 onclick="seleccionarProducto(${p.id}, '${p.nombre}', ${p.precio})">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <strong>${p.nombre}</strong>
                                    <br />
                                    <small class="text-muted">${p.componenteActivo || ''}</small>
                                </div>
                                <div class="text-end">
                                    <span class="badge bg-primary">${formatCurrency(p.precio)}</span>
                                    <br />
                                    <small class="text-muted">Stock: ${p.stock}</small>
                                </div>
                            </div>
                        </button>`
                    ).join('');
                    resultadosDiv.style.display = 'block';
                } else {
                    resultadosDiv.innerHTML = '<div class="list-group-item text-muted">No se encontraron productos</div>';
                    resultadosDiv.style.display = 'block';
                }
            });
    }, posConfig.searchDebounce);
}

function seleccionarProducto(id, nombre, precio) {
    fetch('/Ventas/Carrito?handler=Agregar', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productoID: id, nombre: nombre, precio: precio, cantidad: 1 })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                location.reload();
            } else {
                alert(data.message || 'Error al agregar producto');
            }
        });
}