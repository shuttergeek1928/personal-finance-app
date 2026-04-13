import React from "react";
import { Text, TextProps } from "react-native";
import { cn } from "../../utils/cn";

export const H1 = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-3xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50", className)}
    {...props}
  />
);

export const H2 = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50", className)}
    {...props}
  />
);

export const H3 = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50", className)}
    {...props}
  />
);

export const P = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-base text-zinc-600 dark:text-zinc-400 leading-6", className)}
    {...props}
  />
);

export const Small = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-sm text-zinc-500 dark:text-zinc-500", className)}
    {...props}
  />
);

export const Muted = ({ className, ...props }: TextProps) => (
  <Text
    className={cn("text-sm text-zinc-400 dark:text-zinc-500", className)}
    {...props}
  />
);
