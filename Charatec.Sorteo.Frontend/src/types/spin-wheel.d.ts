
declare module 'spin-wheel' {

  export interface WheelItem {
    label: string;
    weight?: number;
    backgroundColor?: string;
    labelColor?: string;
    value?: string;
  }

  export interface WheelRestEvent {
    currentIndex: number;
  }

  export interface WheelOptions {
    items: WheelItem[];
    isInteractive?: boolean;
    pointerAngle?: number;
    itemLabelFontSizeMax?: number;
    itemLabelColors?: string[];
    itemLabelRadius?: number;
    itemLabelRadiusMax?: number;
    borderWidth?: number;
    borderColor?: string;
    lineWidth?: number;
    lineColor?: string;
    onRest?: (event: WheelRestEvent) => void;
  }

  export class Wheel {

    constructor(
      container: HTMLElement,
      options: WheelOptions
    );

    spinToItem(
      itemIndex: number,
      duration?: number,
      spinToCenter?: boolean,
      numberOfRevolutions?: number,
      direction?: number
    ): void;

    remove(): void;

    getCurrentIndex(): number;
  }
}
