var canvas;

var draggables = new Array(3);
var dragged = null;
var offsetX;
var offsetY;

var JSONviews = null;

var currentView = -1;

function loadImages()
{
  currentView = floor(random(JSONviews.views.length));

  console.log(currentView)

  for (let i = 0; i < 3; ++i) {
    //draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage(JSONviews.views[currentView][i].path);
  }
}

/* P5 */
function preload() {
  JSONviews = loadJSON('./views.json');
}

function setup() {
  canvas = createCanvas(windowWidth, windowHeight);

  for (let i = 0; i < 3; ++i) {
    draggables[i] = new Draggable(createVector(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22), width / 3.1, width / 3.1 * 9 / 16);
  }
}

function draw() {
  background(220);

  if (dragged !== null)
    dragged.setPos(mouseX + offsetX, mouseY + offsetY);
  draggables.forEach(draggable => {
    draggable.draw();  
  });
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
      break;
    }
  }
}

function mouseReleased()
{
  if (dragged === null)
    return;
  dragged = null;
}