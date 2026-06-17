window.videoCableMaps = {
    map: null,
    markers: [],

    initCajasMap: function (elementId, apiKey, cajas) {
        const mapElement = document.getElementById(elementId);

        if (!mapElement) {
            console.error("No se encontró el elemento del mapa:", elementId);
            return;
        }

        if (!apiKey) {
            console.error("No se configuró la API Key de Google Maps.");
            mapElement.innerHTML = "<div style='padding:20px'>No se configuró la API Key de Google Maps.</div>";
            return;
        }

        if (!window.google || !window.google.maps) {
            const script = document.createElement("script");
            script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}`;
            script.async = true;
            script.defer = true;

            script.onload = function () {
                window.videoCableMaps.renderCajasMap(elementId, cajas);
            };

            document.head.appendChild(script);
        } else {
            window.videoCableMaps.renderCajasMap(elementId, cajas);
        }
    },

    renderCajasMap: function (elementId, cajas) {
        const mapElement = document.getElementById(elementId);

        const cajasConCoordenadas = (cajas || []).filter(c =>
            c.latitud !== null &&
            c.longitud !== null &&
            !isNaN(c.latitud) &&
            !isNaN(c.longitud)
        );

        const centroDefault = {
            lat: -26.049,
            lng: -59.936
        };

        const centro = cajasConCoordenadas.length > 0
            ? { lat: cajasConCoordenadas[0].latitud, lng: cajasConCoordenadas[0].longitud }
            : centroDefault;

        this.map = new google.maps.Map(mapElement, {
            center: centro,
            zoom: 14,
            mapTypeId: "roadmap"
        });

        this.markers.forEach(marker => marker.setMap(null));
        this.markers = [];

        const bounds = new google.maps.LatLngBounds();

        cajasConCoordenadas.forEach(caja => {
            const position = {
                lat: caja.latitud,
                lng: caja.longitud
            };

            const marker = new google.maps.Marker({
                position: position,
                map: this.map,
                title: `${caja.codigo} - ${caja.descripcion}`
            });

            const infoWindow = new google.maps.InfoWindow({
                content: `
                    <div style="min-width:220px">
                        <strong>${caja.codigo} - ${caja.descripcion}</strong><br/>
                        <span>${caja.direccion || ""}</span>
                    </div>
                `
            });

            marker.addListener("click", () => {
                infoWindow.open(this.map, marker);
            });

            this.markers.push(marker);
            bounds.extend(position);
        });

        if (cajasConCoordenadas.length > 1) {
            this.map.fitBounds(bounds);
        }
    }
};