/// <reference types="vite/client" />

// Explicitly import React types for JSX
import type * as React from 'react'

declare global {
  namespace JSX {
    interface Element extends React.ReactElement<any, any> {}
    interface ElementClass extends React.Component<any> {
      render(): React.ReactNode
    }
    interface ElementAttributesProperty {
      props: {}
    }
    interface ElementChildrenAttribute {
      children: {}
    }
    interface IntrinsicAttributes extends React.Attributes {}
    interface IntrinsicClassAttributes<T> extends React.ClassAttributes<T> {}

    type IntrinsicElements = {
      [K in keyof React.JSX.IntrinsicElements]: React.JSX.IntrinsicElements[K]
    }
  }
}
