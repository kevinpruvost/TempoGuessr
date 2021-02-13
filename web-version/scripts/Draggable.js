class Draggable {
  constructor(_pos, _width, _height) {
    this.pos = _pos;
    this.width = _width;
    this.height = _height;

    this.image = null;
  }

  setPos(x, y) {
    this.pos.x = x;
    this.pos.y = y;
  }

  setSize(w, h) {
    this.width = w;
    this.height = h;
  }

  setImage(path) {
    loadImage(path, img => {
      this.image = img;
    }, () => {
      console.log('Error while loading image !')
    });
  }

  resetImage() {
    this.image = null;
  }

  hasImage() {
    return this.image !== null;
  }

  isMouseInteracting(mouseX, mouseY) {
    return this.pos.x < mouseX && mouseX < this.pos.x + this.width
      && this.pos.y < mouseY && mouseY < this.pos.y + this.height;
  }

  draw() {
    noStroke();
    fill(0, 0, 0, 40);
    rect(this.pos.x, this.pos.y, this.width, this.height);
    if (!this.hasImage())
      return;
    image(this.image, this.pos.x, this.pos.y, this.width, this.height);
  }
}