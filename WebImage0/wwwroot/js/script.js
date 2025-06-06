
async function GetMemes() {  // функция получения мемов

    const response = await fetch("/api/memes", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok == true) {

        const memes = await response.json();
        var main = document.querySelector("#main");
        var template = document.querySelector("#cardClass");

        // Клонируем новую строку и вставляем её в таблицу

        memes.forEach(meme => {     // шаблонизатор карточек мемов
            var clone = template.content.cloneNode(true);
            $(clone.querySelector('a')).attr('id', meme.id)
            $(clone.querySelector('a')).attr('href', '/meme?id='+meme.id)
            $(clone.querySelector("img")).attr('src', '/img/'+meme.imgUrl);
            clone.querySelector("h2").textContent = meme.name;
            clone.querySelector("p").textContent = meme.description;
            clone.querySelectorAll("button")[0].textContent = meme.tag1;
            clone.querySelectorAll("button")[1].textContent = meme.tag2;
            main.appendChild(clone);

        });
    }

    
}


async function EditMeme() { // функция получения 1 мема
    let params = new URLSearchParams(document.location.search);
    let id = params.get('id');

    const response = await fetch("/api/meme/" + id, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok == true) {

        const meme = await response.json();
        var card = document.querySelector("#EdImg");
        $(card).attr('src', '/img/' + meme.imgUrl);
    }
}

function canRes() { // расчет отрисовка текста в блоке img
    const imgElement = document.getElementById('EdImg');
    const canvas = document.querySelector('canvas');
    const top = document.getElementById('toptext');
    const bottom = document.getElementById('bottomtext');
    const a = 28;

    $(canvas).attr('height', imgElement.naturalHeight); 
    $(canvas).attr('width', imgElement.naturalWidth);
    console.log(`Ширина: ${imgElement.naturalWidth}, Высота: ${imgElement.naturalHeight}`);

    $(top).attr("maxlength", (((imgElement.naturalWidth - imgElement.naturalWidth % a) / a) * 2));
    console.log("maxl = " + ((imgElement.naturalWidth - imgElement.naturalWidth % a) / a) * 2);
    $(bottom).attr("maxlength", ((imgElement.naturalWidth - imgElement.naturalWidth % a) / a) * 2);
    $(top).attr('style', "width: " + (imgElement.clientWidth - (imgElement.clientWidth * 0.08)) + "px;");
    $(bottom).attr('style', "width: " + (imgElement.clientWidth - (imgElement.clientWidth * 0.08)) + "px;");
    
    canDraw();

} 

function canDraw() { // отрисовка изображения с текстом в canvas
    const img = document.getElementById('EdImg');
    const ctx = document.getElementById("canvas").getContext("2d");
    const link = document.getElementById('downlink');
    const vtx = document.getElementById("canvas");
    const top = document.getElementById('toptext').value;
    const bottom = document.getElementById('bottomtext').value;
    const imgElement = document.getElementById('EdImg');
    const height = vtx.height;
    const width = vtx.width;
    const fontSize = height / 14;
    const chan = ((img.naturalWidth - (img.naturalWidth % fontSize*2)) / fontSize*2)+2;
    console.log("chan = " + chan);
    var ta;
    var tb;

    ctx.drawImage(img, 0, 0)
    ctx.font =  fontSize+"px impact";
    ctx.textAlign = "center";
    ctx.fillStyle = "white";
    ctx.strokeStyle = "black";
    ctx.lineWidth = 1.5;

    if (top.length > chan) { // деление строки
        ta = top.slice(0, chan);
        ctx.fillText(ta, width / 2, height / 8);
        ctx.strokeText(ta, width / 2, height / 8);
        tb = top.slice(chan, top.length);
        ctx.fillText(tb, width / 2, (height / 8) + fontSize);
        ctx.strokeText(tb, width / 2, height / 8 + fontSize);
    }
    else { 
        ctx.fillText(top, width / 2, height / 8);
        ctx.strokeText(top, width / 2, height / 8);
    }

    if (bottom.length > chan) {
        ta = bottom.slice(0, chan);
        ctx.fillText(ta, width / 2, height / 1.03 - fontSize);
        ctx.strokeText(ta, width / 2, height / 1.03 - fontSize);
        tb = bottom.slice(chan, bottom.length);
        ctx.fillText(tb, width / 2, height / 1.03);
        ctx.strokeText(tb, width / 2, height / 1.03);
    }
    else { 
        ctx.fillText(bottom, width / 2, height/1.03);
        ctx.strokeText(bottom, width / 2, height/1.03);
    }
    const dataUrl = vtx.toDataURL();
    link.href = dataUrl;
    link.download = 'meme.png';
}

function ImgCng() { // смена картинки в img при загрузке картинки пользователя


    const fileInput = document.getElementById('ImgPath');
    const file = fileInput.files[0];

    if (file != null) {
        const reader = new FileReader();

        // Читаем файл как Data URL
        reader.onload = function (e) {
            const imgElement = document.getElementById('EdImg');
            
            imgElement.src = e.target.result; // Устанавливаем src тега img

        }
        reader.readAsDataURL(file); // Запускаем чтение файла
        
    };

    
}

const dropHint = document.getElementById('drop-zone'); // блок захвата файла при перемещении его на страницу
const fileInput = document.getElementById('ImgPath');

let dragCounter = 0; // для корректного отслеживания dragenter/dragleave по всей странице

// Предотвращаем стандартное поведение для drag/drop на всей странице
['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    window.addEventListener(eventName, e => {
        e.preventDefault();
        e.stopPropagation();
    }, false);
});

// Отслеживаем dragenter и dragleave для показа/скрытия подсказки
window.addEventListener('dragenter', e => {
    dragCounter++;

    // Проверяем, есть ли файлы в событии
    if (e.dataTransfer && e.dataTransfer.types.includes('Files')) {
        dropHint.classList.add('visible');
    }
});

window.addEventListener('dragleave', e => {
    dragCounter--;

    if (dragCounter === 0) {
        dropHint.classList.remove('visible');
    }
});

// При перетаскивании над страницей (dragover) — можно дополнительно показывать подсказку
window.addEventListener('dragover', e => {
    if (e.dataTransfer && e.dataTransfer.types.includes('Files')) {
        dropHint.classList.add('visible');
    }
});

// Обработка сброса файла
window.addEventListener('drop', e => {
    dropHint.classList.remove('visible');

    const files = e.dataTransfer.files;

    if (files.length === 0) return;

    // Берём только первый файл
    const file = files[0];

    // Записываем файл в input[type=file]

    // Создаём DataTransfer для установки файла в input
    const dataTransfer = new DataTransfer();

    dataTransfer.items.add(file);

    fileInput.files = dataTransfer.files;

    

    ImgCng();
    //canRes();
    console.log('Выбран файл:', file.name);


});

async function ai() {  // блок отправки запроса на генерацию мема
    const imgElement = document.getElementById('EdImg');
    const text = document.getElementById("text");
    const a = 28;
    const toptext = document.getElementById("toptext");
    const bottomtext = document.getElementById("bottomtext")
    const b = ((imgElement.naturalWidth - imgElement.naturalWidth % a) / a) * 2;
    const formData = new FormData();
    const img64 = toBase64();
    console.log(img64.length);
    console.log(img64);
    const response = await fetch("/api/ai", {
        method: "post",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
         body: JSON.stringify({
             prompt: text.value,
             length: b,
             imgBase64: img64
         })
    });
    if (response.ok == true) {

        const ss = await response.json();
        console.log(ss);
        toptext.value = ss.top;
        bottomtext.value = ss.bottom;
        canDraw();
    }

}

window.addEventListener('resize', (e) => {
    canRes();
});


function toBase64() { // получение картинки из canvas в формате base64
    const canvas = document.getElementById('canvas');
    document.getElementById('toptext').value = '';
    document.getElementById('bottomtext').value = '';
    canDraw();
    
    return canvas.toDataURL();
}