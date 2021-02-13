var canvas;

var draggables = new Array(3);
var dragged = null;
var offsetX;
var offsetY;

function loadImages()
{
  test = ['images/image0-0.png']

  for (let i = 0; i < 3; ++i) {
    //draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage(test[0]);
  }
}

/* P5 */
function precache() {}

function setup() {
  canvas = createCanvas(windowWidth * 0.99, windowWidth * 0.99 * 9 / 16);

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
  for (let i = 0; i < 3; ++i) {
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