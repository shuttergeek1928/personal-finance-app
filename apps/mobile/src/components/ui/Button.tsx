import React from "react";
import { Pressable, Text, ActivityIndicator } from "react-native";
import { cn } from "../../utils/cn";

interface ButtonProps {
  label: string;
  onPress: () => void;
  variant?: "primary" | "secondary" | "outline" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
  isDisabled?: boolean;
  className?: string;
  labelClassName?: string;
}

const variants = {
  primary: "bg-indigo-600 dark:bg-indigo-500",
  secondary: "bg-zinc-200 dark:bg-zinc-800",
  outline: "bg-transparent border border-zinc-300 dark:border-zinc-700",
  ghost: "bg-transparent",
  danger: "bg-red-600 dark:bg-red-500",
};

const textVariants = {
  primary: "text-white",
  secondary: "text-zinc-900 dark:text-zinc-100",
  outline: "text-zinc-900 dark:text-zinc-100",
  ghost: "text-zinc-900 dark:text-zinc-100",
  danger: "text-white",
};

const sizes = {
  sm: "px-3 py-1.5 rounded-lg",
  md: "px-4 py-3 rounded-xl",
  lg: "px-6 py-4 rounded-2xl",
};

const labelSizes = {
  sm: "text-sm",
  md: "text-base font-semibold",
  lg: "text-lg font-bold",
};

export const Button = ({
  label,
  onPress,
  variant = "primary",
  size = "md",
  isLoading = false,
  isDisabled = false,
  className,
  labelClassName,
}: ButtonProps) => {
  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled || isLoading}
      className={cn(
        "flex-row items-center justify-center active:opacity-70",
        variants[variant],
        sizes[size],
        (isDisabled || isLoading) && "opacity-50",
        className
      )}
    >
      {isLoading ? (
        <ActivityIndicator
          size="small"
          color={variant === "primary" || variant === "danger" ? "white" : "#4f46e5"}
        />
      ) : (
        <Text
          className={cn(
            labelSizes[size],
            textVariants[variant],
            labelClassName
          )}
        >
          {label}
        </Text>
      )}
    </Pressable>
  );
};
