window.telechargerFichier = (nomFichier, typeMime, data) => {
    // data peut être :
    // - une chaîne base64 (selon comment .NET sérialise)
    // - un tableau de nombres (byte[] sérialisé)
    // - un Uint8Array
    let byteArray;

    if (typeof data === 'string') {
        // Chaîne base64
        try {
            const byteCharacters = atob(data);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            byteArray = new Uint8Array(byteNumbers);
        }
        catch (err) {
            console.error('Impossible de décoder la chaîne base64:', err, data);
            return;
        }
    }
    else if (Array.isArray(data)) {
        // Tableau de nombres
        byteArray = new Uint8Array(data);
    }
    else if (data instanceof Uint8Array) {
        byteArray = data;
    }
    else {
        try {
            // Tenter de convertir directement
            byteArray = new Uint8Array(data);
        }
        catch (err) {
            console.error('Format de données non pris en charge pour telechargerFichier:', err, data);
            return;
        }
    }

    const blob = new Blob([byteArray], { type: typeMime });

    // Créer un lien temporaire et déclencher le téléchargement
    const url = URL.createObjectURL(blob);
    const lien = document.createElement('a');
    lien.href = url;
    lien.download = nomFichier;

    document.body.appendChild(lien);
    lien.click();
    document.body.removeChild(lien);

    // Libérer la mémoire
    URL.revokeObjectURL(url);
};

// Lit le contenu texte d'un input[type=file] identifié par son id et retourne le texte
window.readFileAsText = (inputId) => {
    return new Promise((resolve, reject) => {
        const input = document.getElementById(inputId);
        if (!input || !input.files || input.files.length === 0) {
            resolve('');
            return;
        }

        const file = input.files[0];
        const reader = new FileReader();
        reader.onload = function (evt) {
            resolve(evt.target.result);
        };
        reader.onerror = function (err) {
            reject(err);
        };
        reader.readAsText(file);
    });
};
