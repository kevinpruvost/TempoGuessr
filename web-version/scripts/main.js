var canvas;

var draggables = new Array(3);
var dragged = null;
var offsetX;
var offsetY;
var draggedOrigin;

var backgroundImage;

var JSONviews = null;

var currentView = -1;

function loadImages()
{
  currentView = floor(random(JSONviews.views.length));

  indexs = [0, 1, 2];

  shuffle(indexs, true);

  for (let i = 0; i < 3; ++i) {
    //draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage(JSONviews.views[currentView][indexs[i]].path);
  }
}

/* P5 */
function preload() {
  JSONviews = loadJSON('./views.json');

  backgroundImage = loadImage('images/background.jpg');
}

function setup() {
  canvas = createCanvas(windowWidth, windowHeight);

  for (let i = 0; i < 3; ++i) {
    draggables[i] = new Draggable(createVector(width * 0.025 + width / 3.2 * i + (10 * i), height * 0.28), width / 3.2, width / 3.2 * 9 / 16);
  }
}

function draw() {
  background(220);

  image(backgroundImage, 0, 0, width, height);

  draggables.forEach(draggable => {
    draggable.draw();  
  });
  if (dragged !== null) {
    dragged.setPos(mouseX + offsetX, mouseY + offsetY);
    dragged.draw();
  }
}

function keyReleased()
{
  switch (keyCode) {
    case 80:
      print('PLAY')
      loadImages();
      break;
    default:
      break;
  }
}

function mousePressed()
{
  if (dragged !== null)
    return;
  for (let i = 2; i >= 0; i--) {
    if (draggables[i].isMouseInteracting(mouseX, mouseY)) {
      dragged = draggables[i];
      offsetX = draggables[i].pos.x - mouseX;
      offsetY = draggables[i].pos.y - mouseY;
      draggedOriginX = draggables[i].pos.x;
      draggedOriginY = draggables[i].pos.y;
      break;
    }
  }
}

function mouseReleased()
{
  if (dragged === null)
    return;
  for (let i = 2; i >= 0; i--) {
    if (draggables[i] != dragged && draggables[i].isMouseInteracting(mouseX, mouseY)) {
      dragged.setPos(draggables[i].pos.x, draggables[i].pos.y);
      dragged = null;
      draggables[i].setPos(draggedOriginX, draggedOriginY);
      return;
    }
  }
  dragged.setPos(draggedOriginX, draggedOriginY);
  dragged = null;
}